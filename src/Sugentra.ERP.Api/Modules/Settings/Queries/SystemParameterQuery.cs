using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Settings.Queries;

public static class SystemParameterQuery
{
    public const string GetByKeyProcedureName = "usp_Setting_SystemParameters_GetByKey";
}

public record SystemParameterRow(string? ParamValue);

public record SystemParameterListItemDto(
    long Id, string? ParamCategory, string ParamKey, string? ParamValue, string? Description, bool IsActive);

public record SystemParameterListRequest(
    string? Category = null, string? Search = null, bool? IsActive = null, int Page = 1, int PageSize = 10);

/// <summary>Paged read path for Setting_SystemParameters — raw Dapper, no business rules, bypasses
/// CrudUseCase/GenericRepository (those still power the single Create/Update/Delete endpoints).</summary>
public class SystemParameterListQuery(IDbConnectionFactory connectionFactory)
{
    private const string PagedSql = """
        SELECT Id, ParamCategory, ParamKey, ParamValue, Description, IsActive, COUNT(*) OVER() AS TotalCount
        FROM Setting_SystemParameters
        WHERE IsDeleted = 0
          AND (@Category IS NULL OR ParamCategory = @Category)
          AND (@Search IS NULL OR ParamKey LIKE '%' + @Search + '%' OR ParamValue LIKE '%' + @Search + '%' OR Description LIKE '%' + @Search + '%')
          AND (@IsActive IS NULL OR IsActive = @IsActive)
        ORDER BY ParamCategory, ParamKey
        OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
        """;

    private record SystemParameterListRow(
        long Id, string? ParamCategory, string ParamKey, string? ParamValue, string? Description, bool IsActive, int TotalCount);

    public async Task<PagedResult<SystemParameterListItemDto>> GetPagedAsync(SystemParameterListRequest request)
    {
        using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryListAsync<SystemParameterListRow>(PagedSql, request);

        return new PagedResult<SystemParameterListItemDto>
        {
            Items = rows.Select(r => new SystemParameterListItemDto(r.Id, r.ParamCategory, r.ParamKey, r.ParamValue, r.Description, r.IsActive)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = rows.Count > 0 ? rows[0].TotalCount : 0
        };
    }
}
