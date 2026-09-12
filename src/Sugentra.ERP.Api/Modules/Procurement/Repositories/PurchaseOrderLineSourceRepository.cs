using Sugentra.ERP.Api.Modules.Procurement.Entities;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Procurement.Repositories;

public interface IPurchaseOrderLineSourceRepository
{
    Task<IReadOnlyList<PurchaseOrderLineSource>> GetByOrderLineIdsAsync(IEnumerable<long> purchaseOrderLineIds);
    Task<long> AddAsync(PurchaseOrderLineSource entity);
    Task SoftDeleteByOrderLineIdsAsync(IEnumerable<long> purchaseOrderLineIds, long deletedBy);
    Task<IReadOnlyDictionary<long, decimal>> GetOrderedQuantityByRequisitionIdsAsync(IEnumerable<long> requisitionIds);
    Task<IReadOnlyDictionary<long, decimal>> GetOrderedQuantityByRequisitionLineIdsAsync(IEnumerable<long> requisitionLineIds, long? excludeOrderId = null);
}

public class PurchaseOrderLineSourceRepository(IDbConnectionFactory connectionFactory)
    : Repository<PurchaseOrderLineSource>(connectionFactory), IPurchaseOrderLineSourceRepository
{
    public async Task<IReadOnlyList<PurchaseOrderLineSource>> GetByOrderLineIdsAsync(IEnumerable<long> purchaseOrderLineIds)
    {
        var ids = purchaseOrderLineIds.ToList();
        if (ids.Count == 0) return [];

        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Procurement_PurchaseOrderLineSources WHERE PurchaseOrderLineId IN @Ids AND IsDeleted = 0";
        return await connection.QueryListAsync<PurchaseOrderLineSource>(sql, new { Ids = ids });
    }

    public async Task SoftDeleteByOrderLineIdsAsync(IEnumerable<long> purchaseOrderLineIds, long deletedBy)
    {
        var ids = purchaseOrderLineIds.ToList();
        if (ids.Count == 0) return;

        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "UPDATE Procurement_PurchaseOrderLineSources SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy WHERE PurchaseOrderLineId IN @Ids";
        await connection.ExecuteCommandAsync(sql, new { Ids = ids, DeletedBy = deletedBy });
    }

    public async Task<IReadOnlyDictionary<long, decimal>> GetOrderedQuantityByRequisitionIdsAsync(IEnumerable<long> requisitionIds)
    {
        var ids = requisitionIds.ToList();
        if (ids.Count == 0) return new Dictionary<long, decimal>();

        using var connection = ConnectionFactory.CreateConnection();
        const string sql = """
            SELECT PurchaseRequisitionId, SUM(Quantity) AS Quantity
            FROM Procurement_PurchaseOrderLineSources
            WHERE PurchaseRequisitionId IN @RequisitionIds AND IsDeleted = 0
            GROUP BY PurchaseRequisitionId
            """;
        var rows = await connection.QueryListAsync<RequisitionOrderedQuantityRow>(sql, new { RequisitionIds = ids });
        return rows.ToDictionary(r => r.PurchaseRequisitionId, r => r.Quantity);
    }

    // Excludes a given order's own sources so re-saving a Draft PO doesn't double-count its own already-committed quantity.
    public async Task<IReadOnlyDictionary<long, decimal>> GetOrderedQuantityByRequisitionLineIdsAsync(IEnumerable<long> requisitionLineIds, long? excludeOrderId = null)
    {
        var ids = requisitionLineIds.ToList();
        if (ids.Count == 0) return new Dictionary<long, decimal>();

        using var connection = ConnectionFactory.CreateConnection();
        const string sql = """
            SELECT s.PurchaseRequisitionLineId, SUM(s.Quantity) AS Quantity
            FROM Procurement_PurchaseOrderLineSources s
            INNER JOIN Procurement_PurchaseOrderLines l ON l.Id = s.PurchaseOrderLineId
            WHERE s.PurchaseRequisitionLineId IN @LineIds AND s.IsDeleted = 0
              AND (@ExcludeOrderId IS NULL OR l.PurchaseOrderId <> @ExcludeOrderId)
            GROUP BY s.PurchaseRequisitionLineId
            """;
        var rows = await connection.QueryListAsync<RequisitionLineOrderedQuantityRow>(sql, new { LineIds = ids, ExcludeOrderId = excludeOrderId });
        return rows.ToDictionary(r => r.PurchaseRequisitionLineId, r => r.Quantity);
    }

    private record RequisitionOrderedQuantityRow(long PurchaseRequisitionId, decimal Quantity);
    private record RequisitionLineOrderedQuantityRow(long PurchaseRequisitionLineId, decimal Quantity);
}
