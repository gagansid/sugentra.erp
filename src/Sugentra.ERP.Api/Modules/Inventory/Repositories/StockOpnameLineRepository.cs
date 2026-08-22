using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Inventory.Repositories;

public interface IStockOpnameLineRepository
{
    Task<IReadOnlyList<StockOpnameLine>> GetByOpnameIdAsync(long opnameId);
    Task<long> AddAsync(StockOpnameLine entity);
    Task SoftDeleteByOpnameIdAsync(long opnameId, long deletedBy);
}

public class StockOpnameLineRepository(IDbConnectionFactory connectionFactory)
    : Repository<StockOpnameLine>(connectionFactory), IStockOpnameLineRepository
{
    public async Task<IReadOnlyList<StockOpnameLine>> GetByOpnameIdAsync(long opnameId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Inventory_StockOpnameLines WHERE OpnameId = @OpnameId AND IsDeleted = 0";
        return await connection.QueryListAsync<StockOpnameLine>(sql, new { OpnameId = opnameId });
    }

    public async Task SoftDeleteByOpnameIdAsync(long opnameId, long deletedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "UPDATE Inventory_StockOpnameLines SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy WHERE OpnameId = @OpnameId";
        await connection.ExecuteCommandAsync(sql, new { OpnameId = opnameId, DeletedBy = deletedBy });
    }
}
