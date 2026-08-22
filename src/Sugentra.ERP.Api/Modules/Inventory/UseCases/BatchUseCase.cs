using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Modules.Inventory.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Logging;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Inventory.UseCases;

// Wraps the generic CrudUseCase<Batch> but adds a stock guard on delete —
// Batch is otherwise plain reference data, but it's referenced by StockBalance/StockLedger history.
public class BatchUseCase(
    CrudUseCase<Batch> crudUseCase,
    IStockBalanceRepository balanceRepository,
    GenericRepository<Batch> repository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    public Task<Batch?> GetByIdAsync(long id) => crudUseCase.GetByIdAsync(id);

    public Task<IReadOnlyList<Batch>> GetAllAsync() => crudUseCase.GetAllAsync();

    public Task<Batch> CreateAsync(Batch entity) => crudUseCase.CreateAsync(entity);

    public Task<Result<Batch>> UpdateAsync(long id, Batch entity) => crudUseCase.UpdateAsync(id, entity);

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
