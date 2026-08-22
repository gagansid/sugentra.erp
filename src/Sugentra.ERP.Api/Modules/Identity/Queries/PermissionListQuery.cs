namespace Sugentra.ERP.Api.Modules.Identity.Queries;

// SQL text only — no execution here. Repository executes these and returns results (Queries convention).
public static class PermissionListQuery
{
  public const string ExistsByCodeSql = "SELECT COUNT(1) FROM Identity_Permissions WHERE Code = @Code AND IsDeleted = 0";

  // Module is displayed as Setting_Modules.Name (falling back to the legacy code) but the @Module filter
  // matches against Code/legacy code - that's what the UI dropdowns send as the option value.
  public const string GetPagedSql = """
        SELECT p.Id, p.Code, COALESCE(mo.Name, p.Module) AS Module, p.Description, COUNT(*) OVER() AS TotalCount
        FROM Identity_Permissions p
        LEFT JOIN Setting_Modules mo ON mo.Id = p.ModuleId
        WHERE p.IsDeleted = 0
          AND (@Code IS NULL OR p.Code LIKE '%' + @Code + '%')
          AND (@Module IS NULL OR mo.Code = @Module OR p.Module = @Module)
        ORDER BY p.Id
        OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
        """;

  public const string GetAllWithModuleNameSql = """
        SELECT p.Id, p.Code, COALESCE(mo.Name, p.Module) AS Module, p.Description, p.ModuleId,
               p.CreatedAt, p.CreatedBy, p.UpdatedAt, p.UpdatedBy, p.IsDeleted, p.DeletedAt, p.DeletedBy
        FROM Identity_Permissions p
        LEFT JOIN Setting_Modules mo ON mo.Id = p.ModuleId
        WHERE p.IsDeleted = 0
        """;
}
