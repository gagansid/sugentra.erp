using Dapper;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Inventory.Queries;

public record GoodsReceiptAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);

/// <summary>Read path for Goods Receipts adjacent-id navigation — raw Dapper, bypasses GoodsReceiptUseCase entirely.</summary>
public class GoodsReceiptListQuery(IDbConnectionFactory connectionFactory)
{
    private const string AdjacentSql = """
        SELECT (SELECT MAX(Id) FROM Inventory_GoodsReceipts WHERE IsDeleted = 0 AND Id < @Id) AS PreviousId,
               (SELECT MIN(Id) FROM Inventory_GoodsReceipts WHERE IsDeleted = 0 AND Id > @Id) AS NextId,
               (SELECT MIN(Id) FROM Inventory_GoodsReceipts WHERE IsDeleted = 0) AS FirstId,
               (SELECT MAX(Id) FROM Inventory_GoodsReceipts WHERE IsDeleted = 0) AS LastId
        """;

    public async Task<GoodsReceiptAdjacentDto> GetAdjacentAsync(long id)
    {
        using var connection = connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<GoodsReceiptAdjacentDto>(AdjacentSql, new { Id = id });
        return result ?? new GoodsReceiptAdjacentDto(null, null, null, null);
    }
}
