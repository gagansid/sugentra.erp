using Sugentra.ERP.Api.Modules.MasterData.Entities;
using Sugentra.ERP.Api.Modules.MasterData.Queries;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData.Repositories;

public interface IPriceListLineRepository
{
    Task<IReadOnlyList<PriceListLine>> GetByHeaderIdAsync(long priceListHeaderId);
    Task<long> AddAsync(PriceListLine entity);
    Task SoftDeleteByHeaderIdAsync(long priceListHeaderId, long deletedBy);
}

public class PriceListLineRepository(IDbConnectionFactory connectionFactory)
    : Repository<PriceListLine>(connectionFactory), IPriceListLineRepository
{
    public async Task<IReadOnlyList<PriceListLine>> GetByHeaderIdAsync(long priceListHeaderId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryListAsync<PriceListLine>(PriceListLineQuery.GetByHeaderIdSql, new { PriceListHeaderId = priceListHeaderId });
    }

    public async Task SoftDeleteByHeaderIdAsync(long priceListHeaderId, long deletedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(PriceListLineQuery.SoftDeleteByHeaderIdSql, new { PriceListHeaderId = priceListHeaderId, DeletedBy = deletedBy });
    }
}
