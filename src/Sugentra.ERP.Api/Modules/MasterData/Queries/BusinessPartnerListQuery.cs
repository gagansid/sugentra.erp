using Dapper;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData.Queries;

public record BusinessPartnerListItemDto(long Id, string Code, string Name, string PartnerType, string? TaxId, bool IsActive);

public record BusinessPartnerListRequest(string? Keyword = null, int Page = 1, int PageSize = 20, bool? IsActive = null);

public record BusinessPartnerAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);

/// <summary>Read path for Business Partners list — raw Dapper, paged, bypasses BusinessPartnerUseCase entirely
/// (additive alongside the existing GetAll endpoint, does not touch write-side logic). Only the flat header
/// columns shown on the Index table are projected — Addresses/Contacts are not needed for the list view.</summary>
public class BusinessPartnerListQuery(IDbConnectionFactory connectionFactory)
{
    private const string PagedSql = """
        SELECT Id, Code, Name, PartnerType, TaxId, IsActive, COUNT(*) OVER() AS TotalCount
        FROM MasterData_BusinessPartners
        WHERE IsDeleted = 0
          AND (@Keyword IS NULL OR Code LIKE '%' + @Keyword + '%' OR Name LIKE '%' + @Keyword + '%' OR PartnerType LIKE '%' + @Keyword + '%')
          AND (@IsActive IS NULL OR IsActive = @IsActive)
        ORDER BY Id
        OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
        """;

    private record BusinessPartnerListRow(long Id, string Code, string Name, string PartnerType, string? TaxId, bool IsActive, int TotalCount);

    private const string AdjacentSql = """
        SELECT (SELECT MAX(Id) FROM MasterData_BusinessPartners WHERE IsDeleted = 0 AND Id < @Id) AS PreviousId,
               (SELECT MIN(Id) FROM MasterData_BusinessPartners WHERE IsDeleted = 0 AND Id > @Id) AS NextId,
               (SELECT MIN(Id) FROM MasterData_BusinessPartners WHERE IsDeleted = 0) AS FirstId,
               (SELECT MAX(Id) FROM MasterData_BusinessPartners WHERE IsDeleted = 0) AS LastId
        """;

    public async Task<BusinessPartnerAdjacentDto> GetAdjacentAsync(long id)
    {
        using var connection = connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<BusinessPartnerAdjacentDto>(AdjacentSql, new { Id = id });
        return result ?? new BusinessPartnerAdjacentDto(null, null, null, null);
    }

    public async Task<PagedResult<BusinessPartnerListItemDto>> GetPagedAsync(BusinessPartnerListRequest request)
    {
        using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryListAsync<BusinessPartnerListRow>(PagedSql, request);

        return new PagedResult<BusinessPartnerListItemDto>
        {
            Items = rows.Select(r => new BusinessPartnerListItemDto(r.Id, r.Code, r.Name, r.PartnerType, r.TaxId, r.IsActive)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = rows.Count > 0 ? rows[0].TotalCount : 0
        };
    }
}
