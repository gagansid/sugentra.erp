using Dapper;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData.Queries;

public record PriceListListItemDto(long Id, string Name, string Type, int LineCount, bool IsActive);

public record PriceListListRequest(string? Keyword = null, int Page = 1, int PageSize = 20, bool? IsActive = null);

public record PriceListAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);

/// <summary>Read path for Price Lists list — raw Dapper, paged, bypasses PriceListUseCase entirely (additive
/// alongside the existing GetAll endpoint, does not touch write-side logic). LineCount is a correlated subquery
/// instead of a join since only the count is needed for the Index table, not the individual lines.</summary>
public class PriceListListQuery(IDbConnectionFactory connectionFactory)
{
    private const string PagedSql = """
        SELECT h.Id, h.Name, h.Type, h.IsActive,
               (SELECT COUNT(*) FROM MasterData_PriceListLines l WHERE l.PriceListHeaderId = h.Id AND l.IsDeleted = 0) AS LineCount,
               COUNT(*) OVER() AS TotalCount
        FROM MasterData_PriceListHeaders h
        WHERE h.IsDeleted = 0
          AND (@Keyword IS NULL OR h.Name LIKE '%' + @Keyword + '%' OR h.Type LIKE '%' + @Keyword + '%')
          AND (@IsActive IS NULL OR h.IsActive = @IsActive)
        ORDER BY h.Id
        OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
        """;

    private record PriceListRow(long Id, string Name, string Type, bool IsActive, int LineCount, int TotalCount);

    private const string AdjacentSql = """
        SELECT (SELECT MAX(Id) FROM MasterData_PriceListHeaders WHERE IsDeleted = 0 AND Id < @Id) AS PreviousId,
               (SELECT MIN(Id) FROM MasterData_PriceListHeaders WHERE IsDeleted = 0 AND Id > @Id) AS NextId,
               (SELECT MIN(Id) FROM MasterData_PriceListHeaders WHERE IsDeleted = 0) AS FirstId,
               (SELECT MAX(Id) FROM MasterData_PriceListHeaders WHERE IsDeleted = 0) AS LastId
        """;

    public async Task<PriceListAdjacentDto> GetAdjacentAsync(long id)
    {
        using var connection = connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<PriceListAdjacentDto>(AdjacentSql, new { Id = id });
        return result ?? new PriceListAdjacentDto(null, null, null, null);
    }

    public async Task<PagedResult<PriceListListItemDto>> GetPagedAsync(PriceListListRequest request)
    {
        using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryListAsync<PriceListRow>(PagedSql, request);

        return new PagedResult<PriceListListItemDto>
        {
            Items = rows.Select(r => new PriceListListItemDto(r.Id, r.Name, r.Type, r.LineCount, r.IsActive)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = rows.Count > 0 ? rows[0].TotalCount : 0
        };
    }
}
