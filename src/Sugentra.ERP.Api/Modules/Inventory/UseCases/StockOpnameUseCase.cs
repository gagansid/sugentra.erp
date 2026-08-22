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

public class StockOpnameUseCase(
    GenericRepository<StockOpname> opnameRepository,
    IStockOpnameLineRepository lineRepository,
    GenericRepository<StockLedger> ledgerRepository,
    IStockBalanceRepository balanceRepository,
    IDocumentNumberGeneratorService documentNumberGeneratorService,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    public async Task<IReadOnlyList<StockOpnameResponse>> GetAllAsync()
    {
        var opnames = await opnameRepository.GetAllAsync();
        return await Task.WhenAll(opnames.Select(ToResponseAsync));
    }

    public async Task<StockOpnameResponse?> GetByIdAsync(long id)
    {
        var opname = await opnameRepository.GetByIdAsync(id);
        return opname is null ? null : await ToResponseAsync(opname);
    }

    public async Task<StockOpnameResponse> CreateAsync(CreateStockOpnameRequest request)
    {
        var number = await documentNumberGeneratorService.GetNextAsync("StockOpname");

        var opname = new StockOpname
        {
            OpnameNumber = number.FormattedNumber,
            WarehouseId = request.WarehouseId,
            OpnameDate = request.OpnameDate,
            Status = "Draft",
            Notes = request.Notes,
            CreatedBy = currentUserService.UserId
        };

        var id = await opnameRepository.AddAsync(opname);
        await AddLinesAsync(id, request.Lines);

        var response = await ToResponseAsync(opname);
        await auditLogService.LogAsync("Inventory_StockOpnames", id, "Create", null, JsonSerializer.Serialize(response), currentUserService.UserId);

        return response;
    }

    public async Task<Result<StockOpnameResponse>> UpdateAsync(long id, UpdateStockOpnameRequest request)
    {
        var opname = await opnameRepository.GetByIdAsync(id);
        if (opname is null)
        {
            return Result<StockOpnameResponse>.Failure($"StockOpname {id} not found.");
        }

        if (opname.Status != "Draft")
        {
            return Result<StockOpnameResponse>.Failure("Only Draft stock opnames can be edited.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(opname));

        opname.OpnameDate = request.OpnameDate;
        opname.Notes = request.Notes;
        opname.UpdatedBy = currentUserService.UserId;
        opname.UpdatedAt = DateTime.UtcNow;

        await opnameRepository.UpdateAsync(opname);

        // Full line-set replace — simplest correct behavior, avoids incremental add/remove diffing.
        await lineRepository.SoftDeleteByOpnameIdAsync(id, currentUserService.UserId ?? 0);
        await AddLinesAsync(id, request.Lines);

        var response = await ToResponseAsync(opname);
        await auditLogService.LogAsync("Inventory_StockOpnames", id, "Update", oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<StockOpnameResponse>.Success(response);
    }

    public Task<Result<StockOpnameResponse>> SubmitAsync(long id) => TransitionStatusAsync(id, "Draft", "Submitted");

    public async Task<Result<StockOpnameResponse>> ApproveAsync(long id)
    {
        var result = await TransitionStatusAsync(id, "Submitted", "Approved");
        if (result.IsSuccess)
        {
            await ApplyVarianceAdjustmentsAsync(id);
        }

        return result;
    }

    // Posts variance quantities to the ledger/balance only once the count is Approved, never on Submit.
    private async Task ApplyVarianceAdjustmentsAsync(long opnameId)
    {
        var opname = await opnameRepository.GetByIdAsync(opnameId);
        if (opname is null)
        {
            return;
        }

        var lines = await lineRepository.GetByOpnameIdAsync(opnameId);
        foreach (var line in lines.Where(l => l.VarianceQuantity != 0))
        {
            var balance = await balanceRepository.FindAsync(line.ItemId, opname.WarehouseId, line.BatchId);
            if (balance is null)
            {
                continue;
            }

            balance.QuantityOnHand += line.VarianceQuantity;
            balance.UpdatedBy = currentUserService.UserId;
            balance.UpdatedAt = DateTime.UtcNow;
            await balanceRepository.UpdateAsync(balance);

            await ledgerRepository.AddAsync(new StockLedger
            {
                ItemId = line.ItemId,
                WarehouseId = opname.WarehouseId,
                BatchId = line.BatchId,
                MovementType = "Adjustment",
                QuantityChange = line.VarianceQuantity,
                UnitCost = balance.AverageCost,
                ReferenceType = "StockOpname",
                ReferenceId = opname.Id,
                MovementDate = opname.OpnameDate,
                CreatedBy = currentUserService.UserId
            });
        }
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var opname = await opnameRepository.GetByIdAsync(id);
        if (opname is null)
        {
            return Result<bool>.Failure($"StockOpname {id} not found.");
        }

        if (opname.Status != "Draft")
        {
            return Result<bool>.Failure("Only Draft stock opnames can be deleted.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(opname));

        await lineRepository.SoftDeleteByOpnameIdAsync(id, currentUserService.UserId ?? 0);
        await opnameRepository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);

        await auditLogService.LogAsync("Inventory_StockOpnames", id, "SoftDelete", oldValues, null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    private async Task<Result<StockOpnameResponse>> TransitionStatusAsync(long id, string requiredStatus, string newStatus)
    {
        var opname = await opnameRepository.GetByIdAsync(id);
        if (opname is null)
        {
            return Result<StockOpnameResponse>.Failure($"StockOpname {id} not found.");
        }

        if (opname.Status != requiredStatus)
        {
            return Result<StockOpnameResponse>.Failure($"Only {requiredStatus} stock opnames can transition to {newStatus}.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(opname));

        opname.Status = newStatus;
        opname.UpdatedBy = currentUserService.UserId;
        opname.UpdatedAt = DateTime.UtcNow;
        await opnameRepository.UpdateAsync(opname);

        var response = await ToResponseAsync(opname);
        await auditLogService.LogAsync("Inventory_StockOpnames", id, newStatus, oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<StockOpnameResponse>.Success(response);
    }

    private async Task AddLinesAsync(long opnameId, List<StockOpnameLineRequest> lines)
    {
        foreach (var line in lines)
        {
            await lineRepository.AddAsync(new StockOpnameLine
            {
                OpnameId = opnameId,
                ItemId = line.ItemId,
                BatchId = line.BatchId,
                SystemQuantity = line.SystemQuantity,
                CountedQuantity = line.CountedQuantity,
                VarianceQuantity = line.CountedQuantity - line.SystemQuantity,
                Notes = line.Notes,
                CreatedBy = currentUserService.UserId
            });
        }
    }

    private async Task<StockOpnameResponse> ToResponseAsync(StockOpname opname)
    {
        var lines = await lineRepository.GetByOpnameIdAsync(opname.Id);
        var lineResponses = lines.Select(l => new StockOpnameLineResponse(l.Id, l.ItemId, l.BatchId, l.SystemQuantity, l.CountedQuantity, l.VarianceQuantity, l.Notes)).ToList();
        return new StockOpnameResponse(opname.Id, opname.OpnameNumber, opname.WarehouseId, opname.OpnameDate, opname.Status, opname.Notes, opname.CreatedAt, lineResponses);
    }
}
