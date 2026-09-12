using Sugentra.ERP.Api.Modules.Procurement.Entities;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Procurement.Repositories;

public interface IPurchaseOrderLineRepository
{
    Task<IReadOnlyList<PurchaseOrderLine>> GetByOrderIdAsync(long purchaseOrderId);
    Task<long> AddAsync(PurchaseOrderLine entity);
    Task SoftDeleteByOrderIdAsync(long purchaseOrderId, long deletedBy);
    Task UpdateReceivedQuantityAsync(long lineId, decimal receivedQuantity);
}

public class PurchaseOrderLineRepository(IDbConnectionFactory connectionFactory)
    : Repository<PurchaseOrderLine>(connectionFactory), IPurchaseOrderLineRepository
{
    public async Task<IReadOnlyList<PurchaseOrderLine>> GetByOrderIdAsync(long purchaseOrderId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Procurement_PurchaseOrderLines WHERE PurchaseOrderId = @PurchaseOrderId AND IsDeleted = 0";
        return await connection.QueryListAsync<PurchaseOrderLine>(sql, new { PurchaseOrderId = purchaseOrderId });
    }

    public async Task SoftDeleteByOrderIdAsync(long purchaseOrderId, long deletedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "UPDATE Procurement_PurchaseOrderLines SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy WHERE PurchaseOrderId = @PurchaseOrderId";
        await connection.ExecuteCommandAsync(sql, new { PurchaseOrderId = purchaseOrderId, DeletedBy = deletedBy });
    }

    // Called by Inventory (via IPurchaseOrderReceiptService) after a Goods Receipt referencing this PO is posted.
    public async Task UpdateReceivedQuantityAsync(long lineId, decimal receivedQuantity)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "UPDATE Procurement_PurchaseOrderLines SET ReceivedQuantity = ReceivedQuantity + @ReceivedQuantity WHERE Id = @LineId";
        await connection.ExecuteCommandAsync(sql, new { LineId = lineId, ReceivedQuantity = receivedQuantity });
    }
}
