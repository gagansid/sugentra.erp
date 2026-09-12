using Sugentra.ERP.Api.Modules.Procurement.Entities;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Procurement.Repositories;

public interface IPurchaseOrderRequisitionRepository
{
    Task<IReadOnlyList<PurchaseOrderRequisition>> GetByOrderIdAsync(long purchaseOrderId);
    Task<IReadOnlyDictionary<long, IReadOnlyList<long>>> GetRequisitionIdsByOrderIdsAsync(IEnumerable<long> purchaseOrderIds);
    Task<IReadOnlyDictionary<long, IReadOnlyList<long>>> GetOrderIdsByRequisitionIdsAsync(IEnumerable<long> requisitionIds);
    Task<long> AddAsync(PurchaseOrderRequisition entity);
    Task SoftDeleteByOrderIdAsync(long purchaseOrderId, long deletedBy);
}

public class PurchaseOrderRequisitionRepository(IDbConnectionFactory connectionFactory)
    : Repository<PurchaseOrderRequisition>(connectionFactory), IPurchaseOrderRequisitionRepository
{
    public async Task<IReadOnlyList<PurchaseOrderRequisition>> GetByOrderIdAsync(long purchaseOrderId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Procurement_PurchaseOrderRequisitions WHERE PurchaseOrderId = @PurchaseOrderId AND IsDeleted = 0";
        return await connection.QueryListAsync<PurchaseOrderRequisition>(sql, new { PurchaseOrderId = purchaseOrderId });
    }

    public async Task<IReadOnlyDictionary<long, IReadOnlyList<long>>> GetRequisitionIdsByOrderIdsAsync(IEnumerable<long> purchaseOrderIds)
    {
        var ids = purchaseOrderIds as IReadOnlyCollection<long> ?? purchaseOrderIds.ToList();
        if (ids.Count == 0) return new Dictionary<long, IReadOnlyList<long>>();

        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Procurement_PurchaseOrderRequisitions WHERE PurchaseOrderId IN @PurchaseOrderIds AND IsDeleted = 0";
        var rows = await connection.QueryListAsync<PurchaseOrderRequisition>(sql, new { PurchaseOrderIds = ids });
        return rows.GroupBy(r => r.PurchaseOrderId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<long>)g.Select(r => r.PurchaseRequisitionId).ToList());
    }

    public async Task<IReadOnlyDictionary<long, IReadOnlyList<long>>> GetOrderIdsByRequisitionIdsAsync(IEnumerable<long> requisitionIds)
    {
        var ids = requisitionIds as IReadOnlyCollection<long> ?? requisitionIds.ToList();
        if (ids.Count == 0) return new Dictionary<long, IReadOnlyList<long>>();

        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Procurement_PurchaseOrderRequisitions WHERE PurchaseRequisitionId IN @PurchaseRequisitionIds AND IsDeleted = 0";
        var rows = await connection.QueryListAsync<PurchaseOrderRequisition>(sql, new { PurchaseRequisitionIds = ids });
        return rows.GroupBy(r => r.PurchaseRequisitionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<long>)g.Select(r => r.PurchaseOrderId).ToList());
    }

    public async Task SoftDeleteByOrderIdAsync(long purchaseOrderId, long deletedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "UPDATE Procurement_PurchaseOrderRequisitions SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy WHERE PurchaseOrderId = @PurchaseOrderId";
        await connection.ExecuteCommandAsync(sql, new { PurchaseOrderId = purchaseOrderId, DeletedBy = deletedBy });
    }
}
