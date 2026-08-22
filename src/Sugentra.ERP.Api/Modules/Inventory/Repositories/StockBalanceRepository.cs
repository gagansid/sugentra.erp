using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Inventory.Repositories;

public interface IStockBalanceRepository
{
    Task<StockBalance?> FindAsync(long itemId, long warehouseId, long? batchId);
    Task<long> AddAsync(StockBalance entity);
    Task UpdateAsync(StockBalance entity);
    Task<decimal> GetTotalQuantityByBatchAsync(long batchId);
}

public class StockBalanceRepository(IDbConnectionFactory connectionFactory)
    : Repository<StockBalance>(connectionFactory), IStockBalanceRepository
{
    public async Task<StockBalance?> FindAsync(long itemId, long warehouseId, long? batchId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = """
            SELECT * FROM Inventory_StockBalances
            WHERE ItemId = @ItemId AND WarehouseId = @WarehouseId
              AND ((@BatchId IS NULL AND BatchId IS NULL) OR BatchId = @BatchId)
              AND IsDeleted = 0
            """;
        var results = await connection.QueryListAsync<StockBalance>(sql, new { ItemId = itemId, WarehouseId = warehouseId, BatchId = batchId });
        return results.FirstOrDefault();
    }

    public async Task<decimal> GetTotalQuantityByBatchAsync(long batchId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = """
            SELECT ISNULL(SUM(Quantity), 0) FROM Inventory_StockBalances
            WHERE BatchId = @BatchId AND IsDeleted = 0
            """;
        return await connection.QueryScalarAsync<decimal>(sql, new { BatchId = batchId });
    }
}
