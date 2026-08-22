using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Inventory.Repositories;

public interface IGoodsReceiptLineRepository
{
    Task<IReadOnlyList<GoodsReceiptLine>> GetByReceiptIdAsync(long receiptId);
    Task<long> AddAsync(GoodsReceiptLine entity);
    Task SoftDeleteByReceiptIdAsync(long receiptId, long deletedBy);
}

public class GoodsReceiptLineRepository(IDbConnectionFactory connectionFactory)
    : Repository<GoodsReceiptLine>(connectionFactory), IGoodsReceiptLineRepository
{
    public async Task<IReadOnlyList<GoodsReceiptLine>> GetByReceiptIdAsync(long receiptId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Inventory_GoodsReceiptLines WHERE ReceiptId = @ReceiptId AND IsDeleted = 0";
        return await connection.QueryListAsync<GoodsReceiptLine>(sql, new { ReceiptId = receiptId });
    }

    public async Task SoftDeleteByReceiptIdAsync(long receiptId, long deletedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "UPDATE Inventory_GoodsReceiptLines SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy WHERE ReceiptId = @ReceiptId";
        await connection.ExecuteCommandAsync(sql, new { ReceiptId = receiptId, DeletedBy = deletedBy });
    }
}
