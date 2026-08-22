namespace Sugentra.ERP.Api.Modules.Identity.Queries;

// SQL text only — no execution here. Repository executes these and returns results (Queries convention).
public static class UserPermissionQuery
{
    // UQ_Identity_UserPermissions_User_Permission always keeps the row once created, so check/uncheck always reuses
    // this row regardless of IsDeleted — only IsAllowed and the Updated audit columns matter for Allow/Deny state.
    public const string GetOverrideSql = "SELECT * FROM Identity_UserPermissions WHERE UserId = @UserId AND PermissionId = @PermissionId";

    // 2-table join (UserPermissions + Permissions) — below the SP threshold, stays inline.
    public const string GetOverridesByUserIdSql = """
        SELECT p.Code, up.IsAllowed, up.CreatedAt, up.UpdatedAt
        FROM Identity_UserPermissions up
        INNER JOIN Identity_Permissions p ON p.Id = up.PermissionId
        WHERE up.UserId = @UserId AND up.IsDeleted = 0 AND p.IsDeleted = 0
        """;

    public const string UpdateIsAllowedSql = "UPDATE Identity_UserPermissions SET IsAllowed = @IsAllowed, UpdatedAt = GETDATE(), UpdatedBy = @UpdatedBy WHERE Id = @Id";

    public const string RemoveSql = """
        UPDATE Identity_UserPermissions SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy
        WHERE UserId = @UserId AND PermissionId = @PermissionId AND IsDeleted = 0
        """;
}
