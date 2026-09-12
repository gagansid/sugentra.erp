using Dapper;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Procurement.Queries;

public record ProcurementAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);

/// <summary>Shared read path for First/Previous/Next/Last detail-page navigation across Procurement entities — raw Dapper, table names are fixed developer-controlled constants.</summary>
public class ProcurementAdjacentQuery(IDbConnectionFactory connectionFactory)
{
    public Task<ProcurementAdjacentDto> GetPurchaseRequisitionAdjacentAsync(long id) => GetAdjacentAsync("Procurement_PurchaseRequisitions", id);

    public Task<ProcurementAdjacentDto> GetPurchaseOrderAdjacentAsync(long id) => GetAdjacentAsync("Procurement_PurchaseOrders", id);

    private async Task<ProcurementAdjacentDto> GetAdjacentAsync(string table, long id)
    {
        var sql = $"""
            SELECT (SELECT MAX(Id) FROM {table} WHERE IsDeleted = 0 AND Id < @Id) AS PreviousId,
                   (SELECT MIN(Id) FROM {table} WHERE IsDeleted = 0 AND Id > @Id) AS NextId,
                   (SELECT MIN(Id) FROM {table} WHERE IsDeleted = 0) AS FirstId,
                   (SELECT MAX(Id) FROM {table} WHERE IsDeleted = 0) AS LastId
            """;
        using var connection = connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<ProcurementAdjacentDto>(sql, new { Id = id });
        return result ?? new ProcurementAdjacentDto(null, null, null, null);
    }
}
