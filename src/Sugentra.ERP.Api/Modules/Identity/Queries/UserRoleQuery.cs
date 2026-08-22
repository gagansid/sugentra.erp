namespace Sugentra.ERP.Api.Modules.Identity.Queries;

// SQL text only — no execution here. Repository executes these and returns results (Queries convention).
public static class UserRoleQuery
{
    public const string ExistsSql = "SELECT COUNT(1) FROM Identity_UserRoles WHERE UserId = @UserId AND RoleId = @RoleId AND IsDeleted = 0";

    public const string GetRoleIdsByUserIdSql = "SELECT RoleId FROM Identity_UserRoles WHERE UserId = @UserId AND IsDeleted = 0";

    public const string GetUserIdsByRoleIdSql = "SELECT UserId FROM Identity_UserRoles WHERE RoleId = @RoleId AND IsDeleted = 0";

    public const string RevokeSql = """
        UPDATE Identity_UserRoles SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy
        WHERE UserId = @UserId AND RoleId = @RoleId AND IsDeleted = 0
        """;

    // UQ_Identity_UserRoles_User_Role is not filtered by IsDeleted, so a previously revoked row must be reactivated rather than re-inserted.
    public const string GetDeletedIdSql = "SELECT Id FROM Identity_UserRoles WHERE UserId = @UserId AND RoleId = @RoleId AND IsDeleted = 1";

    public const string ReactivateSql = """
        UPDATE Identity_UserRoles
        SET IsDeleted = 0, DeletedAt = NULL, DeletedBy = NULL, CreatedBy = @CreatedBy, CreatedAt = GETDATE()
        WHERE Id = @Id
        """;
}
