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
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    public async Task<IReadOnlyList<StockMutationResponse>> GetAllAsync()
    {
        var mutations = await mutationRepository.GetAllAsync();
        return await Task.WhenAll(mutations.Select(ToResponseAsync));
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

    public Task<Result<StockMutationResponse>> ApproveAsync(long id) => TransitionStatusAsync(id, "Draft", "Approved");

    public async Task<Result<StockMutationResponse>> CompleteAsync(long id)
    {
        var result = await TransitionStatusAsync(id, "Approved", "Completed");
        if (result.IsSuccess)
        {
            await ApplyStockMovementAsync(id);
        }

        return result;
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

    private async Task<Result<StockMutationResponse>> TransitionStatusAsync(long id, string requiredStatus, string newStatus)
    {
        var mutation = await mutationRepository.GetByIdAsync(id);
        if (mutation is null)
        {
            return Result<StockMutationResponse>.Failure($"StockMutation {id} not found.");
        }

        if (mutation.Status != requiredStatus)
        {
            return Result<StockMutationResponse>.Failure($"Only {requiredStatus} mutations can transition to {newStatus}.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(mutation));

        mutation.Status = newStatus;
        mutation.UpdatedBy = currentUserService.UserId;
        mutation.UpdatedAt = DateTime.UtcNow;
        await mutationRepository.UpdateAsync(mutation);

        var response = await ToResponseAsync(mutation);
        await auditLogService.LogAsync("Inventory_StockMutations", id, newStatus, oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<StockMutationResponse>.Success(response);
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
        var lines = await lineRepository.GetByMutationIdAsync(mutation.Id);
        var lineResponses = lines.Select(l => new StockMutationLineResponse(l.Id, l.ItemId, l.BatchId, l.Quantity)).ToList();
        return new StockMutationResponse(
            mutation.Id, mutation.MutationNumber, mutation.MutationType, mutation.SourceWarehouseId, mutation.DestinationWarehouseId,
            mutation.VendorReference, mutation.MutationDate, mutation.Status, mutation.Notes, mutation.CreatedAt, lineResponses);
    }
}
