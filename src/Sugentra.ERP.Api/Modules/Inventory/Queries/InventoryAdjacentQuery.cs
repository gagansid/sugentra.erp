using Dapper;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Inventory.Queries;

public record InventoryAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);

/// <summary>Shared read path for First/Previous/Next/Last detail-page navigation across simple Inventory entities — raw Dapper, table names are fixed developer-controlled constants.</summary>
public class InventoryAdjacentQuery(IDbConnectionFactory connectionFactory)
{
    public Task<InventoryAdjacentDto> GetBatchAdjacentAsync(long id) => GetAdjacentAsync("Inventory_Batches", id);

    public Task<InventoryAdjacentDto> GetQuarantineHoldAdjacentAsync(long id) => GetAdjacentAsync("Inventory_QuarantineHolds", id);

    public Task<InventoryAdjacentDto> GetStockMutationAdjacentAsync(long id) => GetAdjacentAsync("Inventory_StockMutations", id);

    public Task<InventoryAdjacentDto> GetStockOpnameAdjacentAsync(long id) => GetAdjacentAsync("Inventory_StockOpnames", id);

    private async Task<InventoryAdjacentDto> GetAdjacentAsync(string table, long id)
    {
        var sql = $"""
            SELECT (SELECT MAX(Id) FROM {table} WHERE IsDeleted = 0 AND Id < @Id) AS PreviousId,
                   (SELECT MIN(Id) FROM {table} WHERE IsDeleted = 0 AND Id > @Id) AS NextId,
                   (SELECT MIN(Id) FROM {table} WHERE IsDeleted = 0) AS FirstId,
                   (SELECT MAX(Id) FROM {table} WHERE IsDeleted = 0) AS LastId
            """;
        using var connection = connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<InventoryAdjacentDto>(sql, new { Id = id });
        return result ?? new InventoryAdjacentDto(null, null, null, null);
    }
}
