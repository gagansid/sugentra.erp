using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Settings.Queries;

public record AuditLogListItemDto(long Id, string TableName, string Module, string ModuleName, string Menu, string MenuName, long RecordId, string Action, string? OldValues, string? NewValues, long? ChangedBy, string? ChangedByName, DateTime ChangedAt);

public record AuditLogListRequest(string? TableName = null, long? RecordId = null, long? ChangedByUserId = null, string? Action = null, string? Module = null, string? Menu = null, DateTime? FromDate = null, DateTime? ToDate = null, int Page = 1, int PageSize = 20);

public record AuditLogModuleOptionDto(string Code, string Name);

public record AuditLogMenuOptionDto(string Code, string Name);

public record AuditLogFilterOptionsDto(IReadOnlyList<AuditLogModuleOptionDto> Modules, IReadOnlyList<AuditLogMenuOptionDto> Menus);

public record AuditLogAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);

/// <summary>Read path: raw Dapper, no business rules — bypasses UseCase/Repository per the Queries/ convention.
/// Module/Menu aren't stored columns; they're derived by splitting TableName on its first underscore
/// (e.g. "Identity_Users" -> Module "Identity", Menu "Users") since that's the `&lt;Module&gt;_&lt;Entity&gt;` naming
/// convention every audited table follows.</summary>
public class AuditLogQuery(IDbConnectionFactory connectionFactory)
{
    private const string ModuleMenuExpr = """
        CASE WHEN CHARINDEX('_', TableName) > 0 THEN LEFT(TableName, CHARINDEX('_', TableName) - 1) ELSE TableName END AS Module,
        CASE WHEN CHARINDEX('_', TableName) > 0 THEN SUBSTRING(TableName, CHARINDEX('_', TableName) + 1, LEN(TableName)) ELSE '' END AS Menu
        """;

    // Setting_* tables use the singular prefix "Setting" while Setting_Modules.Code is the plural "Settings" - normalize before joining.
    private const string ModuleCodeNormalizeExpr = "CASE WHEN {0} = 'Setting' THEN 'Settings' ELSE {0} END";

    private readonly string _pagedSql = $"""
        WITH Logs AS (
            SELECT Id, TableName, RecordId, Action, OldValues, NewValues, ChangedBy, CreatedBy, ChangedAt, {ModuleMenuExpr}
            FROM Setting_AuditLogs
            WHERE IsDeleted = 0
        )
        SELECT l.Id, l.TableName, l.Module, ISNULL(mo.Name, l.Module) AS ModuleName, l.Menu, ISNULL(me.Name, l.Menu) AS MenuName, l.RecordId, l.Action, l.OldValues, l.NewValues, l.ChangedBy, u.Username AS ChangedByName, l.ChangedAt,
               COUNT(*) OVER() AS TotalCount
        FROM Logs l
        LEFT JOIN Identity_Users u ON u.Id = l.ChangedBy
        LEFT JOIN Setting_Modules mo ON mo.Code = ({string.Format(ModuleCodeNormalizeExpr, "l.Module")})
        LEFT JOIN Setting_Menus me ON me.Controller = l.Menu AND me.ModuleId = mo.Id
        WHERE (
            ((@TableName IS NULL OR l.TableName = @TableName) AND (@RecordId IS NULL OR l.RecordId = @RecordId))
            OR (@ChangedByUserId IS NOT NULL AND (l.ChangedBy = @ChangedByUserId OR l.CreatedBy = @ChangedByUserId))
          )
          AND (@Action IS NULL OR l.Action = @Action)
          AND (@Module IS NULL OR l.Module = @Module)
          AND (@Menu IS NULL OR l.Menu = @Menu)
          AND (@FromDate IS NULL OR l.ChangedAt >= @FromDate)
          AND (@ToDate IS NULL OR l.ChangedAt < DATEADD(DAY, 1, @ToDate))
        ORDER BY l.ChangedAt DESC
        OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
        """;

    private readonly string _byIdSql = $"""
        SELECT l.Id, l.TableName, l.Module, ISNULL(mo.Name, l.Module) AS ModuleName, l.Menu, ISNULL(me.Name, l.Menu) AS MenuName, l.RecordId, l.Action, l.OldValues, l.NewValues, l.ChangedBy, u.Username AS ChangedByName, l.ChangedAt, 0 AS TotalCount
        FROM (
            SELECT Id, TableName, RecordId, Action, OldValues, NewValues, ChangedBy, ChangedAt, {ModuleMenuExpr}
            FROM Setting_AuditLogs
            WHERE IsDeleted = 0 AND Id = @Id
        ) l
        LEFT JOIN Identity_Users u ON u.Id = l.ChangedBy
        LEFT JOIN Setting_Modules mo ON mo.Code = ({string.Format(ModuleCodeNormalizeExpr, "l.Module")})
        LEFT JOIN Setting_Menus me ON me.Controller = l.Menu AND me.ModuleId = mo.Id
        """;

    private readonly string _filterOptionsSql = $"""
        SELECT DISTINCT mm.Module, ISNULL(mo.Name, mm.Module) AS ModuleName, mm.Menu, ISNULL(me.Name, mm.Menu) AS MenuName
        FROM (
            SELECT DISTINCT {ModuleMenuExpr}
            FROM Setting_AuditLogs
            WHERE IsDeleted = 0
        ) mm
        LEFT JOIN Setting_Modules mo ON mo.Code = ({string.Format(ModuleCodeNormalizeExpr, "mm.Module")})
        LEFT JOIN Setting_Menus me ON me.Controller = mm.Menu AND me.ModuleId = mo.Id
        """;


    // @TableName/@RecordId scope navigation to one record's own history; @ChangedByUserId (OR-ed in) additionally
    // includes actions performed by that user elsewhere — mirrors the paged query's scope for the Users Audit Log tab.
    private const string _scopeFilter = "((@TableName IS NULL OR TableName = @TableName) AND (@RecordId IS NULL OR RecordId = @RecordId)) OR (@ChangedByUserId IS NOT NULL AND (ChangedBy = @ChangedByUserId OR CreatedBy = @ChangedByUserId))";

    private readonly string _adjacentSql = $"""
        SELECT (SELECT MAX(Id) FROM Setting_AuditLogs WHERE IsDeleted = 0 AND ChangedAt < c.ChangedAt AND ({_scopeFilter})) AS PreviousId,
               (SELECT MIN(Id) FROM Setting_AuditLogs WHERE IsDeleted = 0 AND ChangedAt > c.ChangedAt AND ({_scopeFilter})) AS NextId,
               (SELECT TOP 1 Id FROM Setting_AuditLogs WHERE IsDeleted = 0 AND ({_scopeFilter}) ORDER BY ChangedAt ASC, Id ASC) AS FirstId,
               (SELECT TOP 1 Id FROM Setting_AuditLogs WHERE IsDeleted = 0 AND ({_scopeFilter}) ORDER BY ChangedAt DESC, Id DESC) AS LastId
        FROM (SELECT ChangedAt FROM Setting_AuditLogs WHERE Id = @Id) c
        """;

    public async Task<PagedResult<AuditLogListItemDto>> GetPagedAsync(AuditLogListRequest request)
    {
        using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryListAsync<AuditLogRow>(_pagedSql, request);

        return new PagedResult<AuditLogListItemDto>
        {
            Items = rows.Select(ToDto).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = rows.Count > 0 ? rows[0].TotalCount : 0
        };
    }

    public async Task<AuditLogListItemDto?> GetByIdAsync(long id)
    {
        using var connection = connectionFactory.CreateConnection();
        var row = await connection.QuerySingleAsync<AuditLogRow?>(_byIdSql, new { Id = id });
        return row is null ? null : ToDto(row);
    }

    public async Task<AuditLogAdjacentDto> GetAdjacentAsync(long id, string? tableName = null, long? recordId = null, long? changedByUserId = null)
    {
        using var connection = connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<AuditLogAdjacentDto>(_adjacentSql, new { Id = id, TableName = tableName, RecordId = recordId, ChangedByUserId = changedByUserId });
        return result ?? new AuditLogAdjacentDto(null, null, null, null);
    }

    public async Task<AuditLogFilterOptionsDto> GetFilterOptionsAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryListAsync<ModuleMenuRow>(_filterOptionsSql, new { });

        return new AuditLogFilterOptionsDto(
            rows.Select(r => new AuditLogModuleOptionDto(r.Module, r.ModuleName)).DistinctBy(m => m.Code).OrderBy(m => m.Name).ToList(),
            rows.Select(r => new AuditLogMenuOptionDto(r.Menu, r.MenuName)).DistinctBy(m => m.Code).OrderBy(m => m.Name).ToList());
    }

    private static AuditLogListItemDto ToDto(AuditLogRow r) =>
        new(r.Id, r.TableName, r.Module, r.ModuleName, r.Menu, r.MenuName, r.RecordId, r.Action, r.OldValues, r.NewValues, r.ChangedBy, r.ChangedByName, r.ChangedAt);

    private record AuditLogRow(long Id, string TableName, string Module, string ModuleName, string Menu, string MenuName, long RecordId, string Action, string? OldValues, string? NewValues, long? ChangedBy, string? ChangedByName, DateTime ChangedAt, int TotalCount = 0);

    private record ModuleMenuRow(string Module, string ModuleName, string Menu, string MenuName);
}

