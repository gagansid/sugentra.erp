using System.Text.Json;
using Sugentra.ERP.Api.Modules.Inventory.Dtos;
using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Modules.Inventory.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Logging;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Inventory.UseCases;

public class StockMutationUseCase(
    GenericRepository<StockMutation> mutationRepository,
    IStockMutationLineRepository lineRepository,
    GenericRepository<StockLedger> ledgerRepository,
    IStockBalanceRepository balanceRepository,
    IDocumentNumberGeneratorService documentNumberGeneratorService,
    IApprovalService approvalService,
    IUserDirectoryService userDirectoryService,
    IWarehouseDirectoryService warehouseDirectoryService,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    public async Task<IReadOnlyList<StockMutationResponse>> GetAllAsync()
    {
        var mutations = await mutationRepository.GetAllAsync();
        var userCache = await ResolveCreatedByUsersAsync(mutations.Select(m => m.CreatedBy));
        return await Task.WhenAll(mutations.Select(m => ToResponseAsync(m, userCache)));
    }

    public async Task<StockMutationResponse?> GetByIdAsync(long id)
    {
        var mutation = await mutationRepository.GetByIdAsync(id);
        return mutation is null ? null : await ToResponseAsync(mutation);
    }

    public async Task<StockMutationResponse> CreateAsync(CreateStockMutationRequest request)
    {
        var number = await documentNumberGeneratorService.GetNextAsync("StockMutation");

        var mutation = new StockMutation
        {
            MutationNumber = number.FormattedNumber,
            MutationType = request.MutationType,
            SourceWarehouseId = request.SourceWarehouseId,
            DestinationWarehouseId = request.DestinationWarehouseId,
            VendorReference = request.VendorReference,
            MutationDate = request.MutationDate,
            Status = "Draft",
            Notes = request.Notes,
            CreatedBy = currentUserService.UserId
        };

        var id = await mutationRepository.AddAsync(mutation);
        await AddLinesAsync(id, request.Lines);

        var response = await ToResponseAsync(mutation);
        await auditLogService.LogAsync("Inventory_StockMutations", id, "Create", null, JsonSerializer.Serialize(response), currentUserService.UserId);

        return response;
    }

    public async Task<Result<StockMutationResponse>> UpdateAsync(long id, UpdateStockMutationRequest request)
    {
        var mutation = await mutationRepository.GetByIdAsync(id);
        if (mutation is null)
        {
            return Result<StockMutationResponse>.Failure($"StockMutation {id} not found.");
        }

        if (mutation.Status != "Draft")
        {
            return Result<StockMutationResponse>.Failure("Only Draft mutations can be edited.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(mutation));

        mutation.MutationType = request.MutationType;
        mutation.SourceWarehouseId = request.SourceWarehouseId;
        mutation.DestinationWarehouseId = request.DestinationWarehouseId;
        mutation.VendorReference = request.VendorReference;
        mutation.MutationDate = request.MutationDate;
        mutation.Notes = request.Notes;
        mutation.UpdatedBy = currentUserService.UserId;
        mutation.UpdatedAt = DateTime.UtcNow;

        await mutationRepository.UpdateAsync(mutation);

        // Full line-set replace — simplest correct behavior, avoids incremental add/remove diffing.
        await lineRepository.SoftDeleteByMutationIdAsync(id, currentUserService.UserId ?? 0);
        await AddLinesAsync(id, request.Lines);

        var response = await ToResponseAsync(mutation);
        await auditLogService.LogAsync("Inventory_StockMutations", id, "Update", oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<StockMutationResponse>.Success(response);
    }

    public async Task<Result<StockMutationResponse>> PostAsync(long id)
    {
        var mutation = await mutationRepository.GetByIdAsync(id);
        if (mutation is null)
        {
            return Result<StockMutationResponse>.Failure($"StockMutation {id} not found.");
        }

        if (mutation.Status != "Draft")
        {
            return Result<StockMutationResponse>.Failure("Only Draft mutations can be posted.");
        }

        var warehouseTypeError = await ValidateWarehouseTypesAsync(mutation);
        if (warehouseTypeError is not null)
        {
            return Result<StockMutationResponse>.Failure(warehouseTypeError);
        }

        // FromVendor mutations bring stock in from an external, unlimited source — no balance check needed.
        if (mutation.MutationType != "FromVendor")
        {
            var insufficiencyError = await ValidateStockSufficiencyAsync(mutation);
            if (insufficiencyError is not null)
            {
                return Result<StockMutationResponse>.Failure(insufficiencyError);
            }
        }

        var submission = await approvalService.SubmitForApprovalAsync(new ApprovalSubmissionRequest(
            "StockMutation", id, mutation.MutationNumber, null, null, mutation.SourceWarehouseId, currentUserService.UserId ?? 0));

        if (submission.RequiresApproval)
        {
            // Status/CurrentApprovalLevel were already set by SetWaitingApprovalLevelAsync, called
            // synchronously from within SubmitForApprovalAsync — re-fetch since our copy is stale.
            var refreshed = await mutationRepository.GetByIdAsync(id);
            return Result<StockMutationResponse>.Success(await ToResponseAsync(refreshed!));
        }

        return await FinalizeCompleteAsync(mutation);
    }

    // Invoked by IApprovalDocumentHandler on submission and every time the request advances to a new level.
    public async Task SetWaitingApprovalLevelAsync(long id, string levelName)
    {
        var mutation = await mutationRepository.GetByIdAsync(id);
        if (mutation is null) return;

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(mutation));

        mutation.Status = "WaitingApproval";
        mutation.CurrentApprovalLevel = levelName;
        mutation.UpdatedAt = DateTime.UtcNow;
        await mutationRepository.UpdateAsync(mutation);

        var response = await ToResponseAsync(mutation);
        await auditLogService.LogAsync("Inventory_StockMutations", id, "ApprovalLevelAdvanced", oldValues, JsonSerializer.Serialize(response), null);
    }

    // Invoked by IApprovalDocumentHandler once every approval level has signed off.
    public async Task CompleteApprovedPostAsync(long id)
    {
        var mutation = await mutationRepository.GetByIdAsync(id);
        if (mutation is null || mutation.Status != "WaitingApproval") return;

        await FinalizeCompleteAsync(mutation);
    }

    // Rejection sends it back to Draft for revision/resubmission — the reason lives in the approval history.
    public async Task RejectPostAsync(long id)
    {
        var mutation = await mutationRepository.GetByIdAsync(id);
        if (mutation is null || mutation.Status != "WaitingApproval") return;

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(mutation));

        mutation.Status = "Draft";
        mutation.CurrentApprovalLevel = null;
        mutation.UpdatedAt = DateTime.UtcNow;
        await mutationRepository.UpdateAsync(mutation);

        var response = await ToResponseAsync(mutation);
        await auditLogService.LogAsync("Inventory_StockMutations", id, "ApprovalRejected", oldValues, JsonSerializer.Serialize(response), null);
    }

    private async Task<Result<StockMutationResponse>> FinalizeCompleteAsync(StockMutation mutation)
    {
        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(mutation));

        mutation.Status = "Completed";
        mutation.CurrentApprovalLevel = null;
        mutation.UpdatedBy = currentUserService.UserId;
        mutation.UpdatedAt = DateTime.UtcNow;
        await mutationRepository.UpdateAsync(mutation);

        await ApplyStockMovementAsync(mutation.Id);

        var response = await ToResponseAsync(mutation);
        await auditLogService.LogAsync("Inventory_StockMutations", mutation.Id, "Completed", oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<StockMutationResponse>.Success(response);
    }

    // Ensures each warehouse in the mutation matches the WarehouseType its MutationType expects
    // (Quarantine-type warehouses must go through the QuarantineHold release flow, not a plain transfer).
    private async Task<string?> ValidateWarehouseTypesAsync(StockMutation mutation)
    {
        var sourceType = await warehouseDirectoryService.GetWarehouseTypeAsync(mutation.SourceWarehouseId);
        var destinationType = mutation.DestinationWarehouseId is long destinationId
            ? await warehouseDirectoryService.GetWarehouseTypeAsync(destinationId)
            : null;

        switch (mutation.MutationType)
        {
            case "Internal":
                if (sourceType == "Quarantine" || destinationType == "Quarantine")
                {
                    return "Quarantine warehouses cannot be used in an Internal stock mutation — release the quarantine hold first.";
                }
                if (sourceType == "Vendor" || destinationType == "Vendor")
                {
                    return "Vendor warehouses can only be used with ToVendor/FromVendor mutation types.";
                }
                break;
            case "ToVendor":
                if (destinationType != "Vendor")
                {
                    return "ToVendor mutations must target a warehouse of type Vendor.";
                }
                break;
            case "FromVendor":
                if (sourceType != "Vendor")
                {
                    return "FromVendor mutations must originate from a warehouse of type Vendor.";
                }
                break;
        }

        return null;
    }

    // Ensures the source warehouse actually holds enough stock before it's decremented on completion.
    private async Task<string?> ValidateStockSufficiencyAsync(StockMutation mutation)
    {
        var lines = await lineRepository.GetByMutationIdAsync(mutation.Id);
        foreach (var line in lines)
        {
            var sourceBalance = await balanceRepository.FindAsync(line.ItemId, mutation.SourceWarehouseId, line.BatchId);
            var available = sourceBalance?.QuantityOnHand ?? 0;
            if (available < line.Quantity)
            {
                return $"Insufficient stock for item {line.ItemId} at warehouse {mutation.SourceWarehouseId}: available {available}, requested {line.Quantity}.";
            }
        }

        return null;
    }

    // Moves stock between warehouses only once a mutation reaches its final Completed state.
    private async Task ApplyStockMovementAsync(long mutationId)
    {
        var mutation = await mutationRepository.GetByIdAsync(mutationId);
        if (mutation is null)
        {
            return;
        }

        var lines = await lineRepository.GetByMutationIdAsync(mutationId);
        foreach (var line in lines)
        {
            var sourceBalance = await balanceRepository.FindAsync(line.ItemId, mutation.SourceWarehouseId, line.BatchId);
            if (sourceBalance is null)
            {
                continue;
            }

            sourceBalance.QuantityOnHand -= line.Quantity;
            sourceBalance.UpdatedBy = currentUserService.UserId;
            sourceBalance.UpdatedAt = DateTime.UtcNow;
            await balanceRepository.UpdateAsync(sourceBalance);

            await ledgerRepository.AddAsync(new StockLedger
            {
                ItemId = line.ItemId,
                WarehouseId = mutation.SourceWarehouseId,
                BatchId = line.BatchId,
                MovementType = mutation.MutationType == "Internal" ? "Mutation" : mutation.MutationType,
                QuantityChange = -line.Quantity,
                UnitCost = sourceBalance.AverageCost,
                ReferenceType = "StockMutation",
                ReferenceId = mutation.Id,
                MovementDate = mutation.MutationDate,
                CreatedBy = currentUserService.UserId
            });

            if (mutation.DestinationWarehouseId is not { } destinationWarehouseId)
            {
                continue;
            }

            var destinationBalance = await balanceRepository.FindAsync(line.ItemId, destinationWarehouseId, line.BatchId);
            if (destinationBalance is null)
            {
                destinationBalance = new StockBalance
                {
                    ItemId = line.ItemId,
                    WarehouseId = destinationWarehouseId,
                    BatchId = line.BatchId,
                    UnitOfMeasurementId = sourceBalance.UnitOfMeasurementId,
                    QuantityOnHand = line.Quantity,
                    AverageCost = sourceBalance.AverageCost,
                    CreatedBy = currentUserService.UserId
                };
                await balanceRepository.AddAsync(destinationBalance);
            }
            else
            {
                var totalQuantity = destinationBalance.QuantityOnHand + line.Quantity;
                destinationBalance.AverageCost = totalQuantity == 0
                    ? destinationBalance.AverageCost
                    : ((destinationBalance.QuantityOnHand * destinationBalance.AverageCost) + (line.Quantity * sourceBalance.AverageCost)) / totalQuantity;
                destinationBalance.QuantityOnHand = totalQuantity;
                destinationBalance.UpdatedBy = currentUserService.UserId;
                destinationBalance.UpdatedAt = DateTime.UtcNow;
                await balanceRepository.UpdateAsync(destinationBalance);
            }

            await ledgerRepository.AddAsync(new StockLedger
            {
                ItemId = line.ItemId,
                WarehouseId = destinationWarehouseId,
                BatchId = line.BatchId,
                MovementType = mutation.MutationType == "Internal" ? "Mutation" : mutation.MutationType,
                QuantityChange = line.Quantity,
                UnitCost = sourceBalance.AverageCost,
                ReferenceType = "StockMutation",
                ReferenceId = mutation.Id,
                MovementDate = mutation.MutationDate,
                CreatedBy = currentUserService.UserId
            });
        }
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var mutation = await mutationRepository.GetByIdAsync(id);
        if (mutation is null)
        {
            return Result<bool>.Failure($"StockMutation {id} not found.");
        }

        if (mutation.Status != "Draft")
        {
            return Result<bool>.Failure("Only Draft mutations can be deleted.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(mutation));

        await lineRepository.SoftDeleteByMutationIdAsync(id, currentUserService.UserId ?? 0);
        await mutationRepository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);

        await auditLogService.LogAsync("Inventory_StockMutations", id, "SoftDelete", oldValues, null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    private async Task AddLinesAsync(long mutationId, List<StockMutationLineRequest> lines)
    {
        foreach (var line in lines)
        {
            await lineRepository.AddAsync(new StockMutationLine
            {
                MutationId = mutationId,
                ItemId = line.ItemId,
                BatchId = line.BatchId,
                Quantity = line.Quantity,
                CreatedBy = currentUserService.UserId
            });
        }
    }

    private async Task<StockMutationResponse> ToResponseAsync(StockMutation mutation)
    {
        var userCache = await ResolveCreatedByUsersAsync([mutation.CreatedBy]);
        return await ToResponseAsync(mutation, userCache);
    }

    private async Task<StockMutationResponse> ToResponseAsync(StockMutation mutation, IReadOnlyDictionary<long, UserDirectoryEntry> userCache)
    {
        var lines = await lineRepository.GetByMutationIdAsync(mutation.Id);
        var lineResponses = lines.Select(l => new StockMutationLineResponse(l.Id, l.ItemId, l.BatchId, l.Quantity)).ToList();
        var createdByUser = mutation.CreatedBy.HasValue && userCache.TryGetValue(mutation.CreatedBy.Value, out var user) ? user : null;
        return new StockMutationResponse(
            mutation.Id, mutation.MutationNumber, mutation.MutationType, mutation.SourceWarehouseId, mutation.DestinationWarehouseId,
            mutation.VendorReference, mutation.MutationDate, mutation.Status, mutation.CurrentApprovalLevel, mutation.Notes,
            mutation.CreatedAt, mutation.CreatedBy, createdByUser?.FullName, lineResponses);
    }

    // Resolves each distinct CreatedBy id once (IUserDirectoryService has no batch lookup) to avoid N+1 calls on list endpoints.
    private async Task<IReadOnlyDictionary<long, UserDirectoryEntry>> ResolveCreatedByUsersAsync(IEnumerable<long?> createdByIds)
    {
        var distinctIds = createdByIds.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        if (distinctIds.Count == 0) return new Dictionary<long, UserDirectoryEntry>();

        var users = await Task.WhenAll(distinctIds.Select(userDirectoryService.GetByIdAsync));
        return distinctIds.Zip(users, (id, user) => (id, user))
            .Where(x => x.user is not null)
            .ToDictionary(x => x.id, x => x.user!);
    }
}
