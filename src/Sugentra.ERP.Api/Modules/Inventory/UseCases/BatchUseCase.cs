using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Modules.Inventory.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Logging;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Inventory.UseCases;

// Wraps the generic CrudUseCase<Batch> but adds a stock guard on delete —
// Batch is otherwise plain reference data, but it's referenced by StockBalance/StockLedger history.
public class BatchUseCase(
    CrudUseCase<Batch> crudUseCase,
    IStockBalanceRepository balanceRepository,
    GenericRepository<Batch> repository,
    IDocumentNumberGeneratorService documentNumberGeneratorService,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    public Task<Batch?> GetByIdAsync(long id) => crudUseCase.GetByIdAsync(id);

    public Task<IReadOnlyList<Batch>> GetAllAsync() => crudUseCase.GetAllAsync();

    // Code is always server-generated (like PO/PR/GR numbers) so it's unique and consistent regardless of client.
    public async Task<Batch> CreateAsync(Batch entity)
    {
        var number = await documentNumberGeneratorService.GetNextAsync("Batch");
        entity.Code = number.FormattedNumber;
        return await crudUseCase.CreateAsync(entity);
    }

    public async Task<Result<Batch>> UpdateAsync(long id, Batch entity)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
        {
            return Result<Batch>.Failure($"Record {id} not found.");
        }

        if (existing.ItemId != entity.ItemId || existing.WarehouseId != entity.WarehouseId)
        {
            var totalQuantity = await balanceRepository.GetTotalQuantityByBatchAsync(id);
            if (totalQuantity != 0)
            {
                return Result<Batch>.Failure("Cannot change Item/Warehouse: this batch already has stock recorded against it.");
            }
        }

        return await crudUseCase.UpdateAsync(id, entity);
    }

    public async Task<bool> HasStockAsync(long id) => await balanceRepository.GetTotalQuantityByBatchAsync(id) != 0;

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
        {
            return Result<bool>.Failure($"Record {id} not found.");
        }

        var totalQuantity = await balanceRepository.GetTotalQuantityByBatchAsync(id);
        if (totalQuantity != 0)
        {
            return Result<bool>.Failure($"Cannot delete batch: it still has {totalQuantity} units in stock.");
        }

        await repository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);
        await auditLogService.LogAsync("Inventory_Batches", id, "SoftDelete",
            System.Text.Json.JsonSerializer.Serialize(existing), null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }
}
