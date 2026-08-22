using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Inventory.Repositories;

public interface IStockMutationLineRepository
{
    Task<IReadOnlyList<StockMutationLine>> GetByMutationIdAsync(long mutationId);
    Task<long> AddAsync(StockMutationLine entity);
    Task SoftDeleteByMutationIdAsync(long mutationId, long deletedBy);
}

public class StockMutationLineRepository(IDbConnectionFactory connectionFactory)
    : Repository<StockMutationLine>(connectionFactory), IStockMutationLineRepository
{
    public async Task<IReadOnlyList<StockMutationLine>> GetByMutationIdAsync(long mutationId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Inventory_StockMutationLines WHERE MutationId = @MutationId AND IsDeleted = 0";
        return await connection.QueryListAsync<StockMutationLine>(sql, new { MutationId = mutationId });
    }

    public async Task SoftDeleteByMutationIdAsync(long mutationId, long deletedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "UPDATE Inventory_StockMutationLines SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy WHERE MutationId = @MutationId";
        await connection.ExecuteCommandAsync(sql, new { MutationId = mutationId, DeletedBy = deletedBy });
    }
}
