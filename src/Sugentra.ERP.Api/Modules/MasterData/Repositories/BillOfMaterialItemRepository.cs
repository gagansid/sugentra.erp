using Sugentra.ERP.Api.Modules.MasterData.Entities;
using Sugentra.ERP.Api.Modules.MasterData.Queries;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData.Repositories;

public interface IBillOfMaterialItemRepository
{
    Task<IReadOnlyList<BillOfMaterialItem>> GetByBomIdAsync(long billOfMaterialId);
    Task<long> AddAsync(BillOfMaterialItem entity);
    Task SoftDeleteByBomIdAsync(long billOfMaterialId, long deletedBy);
}

public class BillOfMaterialItemRepository(IDbConnectionFactory connectionFactory)
    : Repository<BillOfMaterialItem>(connectionFactory), IBillOfMaterialItemRepository
{
    public async Task<IReadOnlyList<BillOfMaterialItem>> GetByBomIdAsync(long billOfMaterialId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryListAsync<BillOfMaterialItem>(BillOfMaterialItemQuery.GetByBomIdSql, new { BillOfMaterialId = billOfMaterialId });
    }

    public async Task SoftDeleteByBomIdAsync(long billOfMaterialId, long deletedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(BillOfMaterialItemQuery.SoftDeleteByBomIdSql, new { BillOfMaterialId = billOfMaterialId, DeletedBy = deletedBy });
    }
}
