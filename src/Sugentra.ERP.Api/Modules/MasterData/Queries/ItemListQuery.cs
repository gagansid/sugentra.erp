using Dapper;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData.Queries;

public record ItemListItemDto(long Id, string Code, string Name, string Category, string? Grade, long UnitOfMeasurementId, decimal StandardPrice, bool IsActive);

public record ItemListRequest(string? Keyword = null, int Page = 1, int PageSize = 20, bool? IsActive = null);

public record ItemAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);

/// <summary>Read path for Items list — raw Dapper, paged, bypasses the generic CrudUseCase&lt;Item&gt;/Repository
/// entirely (additive alongside the existing GetAll endpoint, does not touch write-side logic).</summary>
public class ItemListQuery(IDbConnectionFactory connectionFactory)
{
    private const string PagedSql = """
        SELECT Id, Code, Name, Category, Grade, UnitOfMeasurementId, StandardPrice, IsActive, COUNT(*) OVER() AS TotalCount
        FROM MasterData_Items
        WHERE IsDeleted = 0
          AND (@Keyword IS NULL OR Code LIKE '%' + @Keyword + '%' OR Name LIKE '%' + @Keyword + '%' OR Category LIKE '%' + @Keyword + '%')
          AND (@IsActive IS NULL OR IsActive = @IsActive)
        ORDER BY Id
        OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
        """;

    private record ItemListRow(long Id, string Code, string Name, string Category, string? Grade, long UnitOfMeasurementId, decimal StandardPrice, bool IsActive, int TotalCount);

    private const string AdjacentSql = """
        SELECT (SELECT MAX(Id) FROM MasterData_Items WHERE IsDeleted = 0 AND Id < @Id) AS PreviousId,
               (SELECT MIN(Id) FROM MasterData_Items WHERE IsDeleted = 0 AND Id > @Id) AS NextId,
               (SELECT MIN(Id) FROM MasterData_Items WHERE IsDeleted = 0) AS FirstId,
               (SELECT MAX(Id) FROM MasterData_Items WHERE IsDeleted = 0) AS LastId
        """;

    public async Task<ItemAdjacentDto> GetAdjacentAsync(long id)
    {
        using var connection = connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<ItemAdjacentDto>(AdjacentSql, new { Id = id });
        return result ?? new ItemAdjacentDto(null, null, null, null);
    }

    public async Task<PagedResult<ItemListItemDto>> GetPagedAsync(ItemListRequest request)
    {
        using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryListAsync<ItemListRow>(PagedSql, request);

        return new PagedResult<ItemListItemDto>
        {
            Items = rows.Select(r => new ItemListItemDto(r.Id, r.Code, r.Name, r.Category, r.Grade, r.UnitOfMeasurementId, r.StandardPrice, r.IsActive)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = rows.Count > 0 ? rows[0].TotalCount : 0
        };
    }
}
