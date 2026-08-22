using Dapper;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData.Queries;

public record BillOfMaterialListItemDto(long Id, long ItemId, string Name, string? Description, int LineCount, bool IsActive);

public record BillOfMaterialListRequest(string? Keyword = null, int Page = 1, int PageSize = 20, bool? IsActive = null);

public record BillOfMaterialAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);

/// <summary>Read path for Bill of Materials list — raw Dapper, paged, bypasses BillOfMaterialUseCase entirely
/// (additive alongside the existing GetAll endpoint, does not touch write-side logic). LineCount is a correlated
/// subquery instead of a join since only the count is needed for the Index table, not the individual lines.</summary>
public class BillOfMaterialListQuery(IDbConnectionFactory connectionFactory)
{
    private const string PagedSql = """
        SELECT h.Id, h.ItemId, h.Name, h.Description, h.IsActive,
               (SELECT COUNT(*) FROM MasterData_BillOfMaterialItems l WHERE l.BillOfMaterialId = h.Id AND l.IsDeleted = 0) AS LineCount,
               COUNT(*) OVER() AS TotalCount
        FROM MasterData_BillOfMaterials h
        WHERE h.IsDeleted = 0
          AND (@Keyword IS NULL OR h.Name LIKE '%' + @Keyword + '%' OR h.Description LIKE '%' + @Keyword + '%')
          AND (@IsActive IS NULL OR h.IsActive = @IsActive)
        ORDER BY h.Id
        OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
        """;

    private record BillOfMaterialRow(long Id, long ItemId, string Name, string? Description, bool IsActive, int LineCount, int TotalCount);

    private const string AdjacentSql = """
        SELECT (SELECT MAX(Id) FROM MasterData_BillOfMaterials WHERE IsDeleted = 0 AND Id < @Id) AS PreviousId,
               (SELECT MIN(Id) FROM MasterData_BillOfMaterials WHERE IsDeleted = 0 AND Id > @Id) AS NextId,
               (SELECT MIN(Id) FROM MasterData_BillOfMaterials WHERE IsDeleted = 0) AS FirstId,
               (SELECT MAX(Id) FROM MasterData_BillOfMaterials WHERE IsDeleted = 0) AS LastId
        """;

    public async Task<BillOfMaterialAdjacentDto> GetAdjacentAsync(long id)
    {
        using var connection = connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<BillOfMaterialAdjacentDto>(AdjacentSql, new { Id = id });
        return result ?? new BillOfMaterialAdjacentDto(null, null, null, null);
    }

    public async Task<PagedResult<BillOfMaterialListItemDto>> GetPagedAsync(BillOfMaterialListRequest request)
    {
        using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryListAsync<BillOfMaterialRow>(PagedSql, request);

        return new PagedResult<BillOfMaterialListItemDto>
        {
            Items = rows.Select(r => new BillOfMaterialListItemDto(r.Id, r.ItemId, r.Name, r.Description, r.LineCount, r.IsActive)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = rows.Count > 0 ? rows[0].TotalCount : 0
        };
    }
}
