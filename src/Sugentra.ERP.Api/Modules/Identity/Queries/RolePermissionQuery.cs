namespace Sugentra.ERP.Api.Modules.Identity.Queries;

// SQL text only — no execution here. Repository executes these and returns results (Queries convention).
public static class RolePermissionQuery
{
    public const string ExistsSql = "SELECT COUNT(1) FROM Identity_RolePermissions WHERE RoleId = @RoleId AND PermissionId = @PermissionId AND IsDeleted = 0 AND IsActive = 1";

    // 2-table join (RolePermissions + Permissions) — below the SP threshold (needs ≥3 joins), stays inline.
    public const string GetPermissionCodesByRoleIdsSql = """
        SELECT DISTINCT p.Code
        FROM Identity_RolePermissions rp
        INNER JOIN Identity_Permissions p ON p.Id = rp.PermissionId
        WHERE rp.RoleId IN @RoleIds AND rp.IsDeleted = 0 AND rp.IsActive = 1 AND p.IsDeleted = 0
        """;

    public const string RevokeSql = """
        UPDATE Identity_RolePermissions SET IsActive = 0, UpdatedAt = GETDATE(), UpdatedBy = @UpdatedBy
        WHERE RoleId = @RoleId AND PermissionId = @PermissionId AND IsDeleted = 0 AND IsActive = 1
        """;

    // UQ_Identity_RolePermissions_Role_Permission always keeps the row once created (regardless of IsDeleted/IsActive),
    // so re-assigning a previously revoked/deleted permission must reactivate that row rather than insert a duplicate.
    public const string GetInactiveIdSql = "SELECT Id FROM Identity_RolePermissions WHERE RoleId = @RoleId AND PermissionId = @PermissionId AND (IsDeleted = 1 OR IsActive = 0)";

    public const string ActivateSql = """
        UPDATE Identity_RolePermissions SET IsActive = 1, IsDeleted = 0, DeletedAt = NULL, DeletedBy = NULL, UpdatedAt = GETDATE(), UpdatedBy = @UpdatedBy
        WHERE Id = @Id
        """;

    // 3-table join (Permissions + RolePermissions scoped to one role + Setting_Modules for display name).
    public const string GetPagedForRoleSql = """
        SELECT p.Id, p.Code, COALESCE(mo.Name, p.Module) AS Module, p.Description,
               CASE WHEN rp.Id IS NOT NULL AND rp.IsActive = 1 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsAssigned,
               COALESCE(rp.UpdatedAt, rp.CreatedAt) AS ModifiedAt,
               COUNT(*) OVER() AS TotalCount
        FROM Identity_Permissions p
        LEFT JOIN Setting_Modules mo ON mo.Id = p.ModuleId
        LEFT JOIN Identity_RolePermissions rp ON rp.PermissionId = p.Id AND rp.RoleId = @RoleId AND rp.IsDeleted = 0
        WHERE p.IsDeleted = 0
          AND (@Module IS NULL OR mo.Code = @Module OR p.Module = @Module)
          AND (@Code IS NULL OR p.Code LIKE '%' + @Code + '%')
          AND (@Description IS NULL OR p.Description LIKE '%' + @Description + '%')
          AND (@IsAssigned IS NULL OR (CASE WHEN rp.Id IS NOT NULL THEN 1 ELSE 0 END) = @IsAssigned)
        ORDER BY COALESCE(mo.Name, p.Module), p.Code
        OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
        """;
}
