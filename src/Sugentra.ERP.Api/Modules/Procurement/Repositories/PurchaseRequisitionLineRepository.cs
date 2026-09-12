using Sugentra.ERP.Api.Modules.Procurement.Entities;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Procurement.Repositories;

public interface IPurchaseRequisitionLineRepository
{
    Task<IReadOnlyList<PurchaseRequisitionLine>> GetByRequisitionIdAsync(long requisitionId);
    Task<IReadOnlyList<PurchaseRequisitionLine>> GetByIdsAsync(IEnumerable<long> ids);
    Task<long> AddAsync(PurchaseRequisitionLine entity);
    Task SoftDeleteByRequisitionIdAsync(long requisitionId, long deletedBy);
}

public class PurchaseRequisitionLineRepository(IDbConnectionFactory connectionFactory)
    : Repository<PurchaseRequisitionLine>(connectionFactory), IPurchaseRequisitionLineRepository
{
    public async Task<IReadOnlyList<PurchaseRequisitionLine>> GetByRequisitionIdAsync(long requisitionId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Procurement_PurchaseRequisitionLines WHERE RequisitionId = @RequisitionId AND IsDeleted = 0";
        return await connection.QueryListAsync<PurchaseRequisitionLine>(sql, new { RequisitionId = requisitionId });
    }

    public async Task<IReadOnlyList<PurchaseRequisitionLine>> GetByIdsAsync(IEnumerable<long> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0) return [];

        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Procurement_PurchaseRequisitionLines WHERE Id IN @Ids AND IsDeleted = 0";
        return await connection.QueryListAsync<PurchaseRequisitionLine>(sql, new { Ids = idList });
    }

    public async Task SoftDeleteByRequisitionIdAsync(long requisitionId, long deletedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "UPDATE Procurement_PurchaseRequisitionLines SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy WHERE RequisitionId = @RequisitionId";
        await connection.ExecuteCommandAsync(sql, new { RequisitionId = requisitionId, DeletedBy = deletedBy });
    }
}
