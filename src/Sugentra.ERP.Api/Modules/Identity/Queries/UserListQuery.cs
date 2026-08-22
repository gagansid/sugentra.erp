namespace Sugentra.ERP.Api.Modules.Identity.Queries;

// SQL text only — no execution here. Repository executes these and returns results (Queries convention).
public static class UserListQuery
{
    public const string GetByUsernameSql = "SELECT * FROM Identity_Users WHERE Username = @Username AND IsDeleted = 0";
    // Login accepts either username or email in the same field.
    public const string GetByUsernameOrEmailSql = "SELECT * FROM Identity_Users WHERE (Username = @UsernameOrEmail OR Email = @UsernameOrEmail) AND IsDeleted = 0";
    public const string ExistsByUsernameSql = "SELECT COUNT(1) FROM Identity_Users WHERE Username = @Username AND IsDeleted = 0";
    public const string ExistsByEmailSql = "SELECT COUNT(1) FROM Identity_Users WHERE Email = @Email AND IsDeleted = 0";
    public const string ExistsByEmailForOtherUserSql = "SELECT COUNT(1) FROM Identity_Users WHERE Email = @Email AND Id <> @ExcludeId AND IsDeleted = 0";

    // 3 optional filters crosses the SP threshold rule (many optional filters needing dynamic WHERE) — moved to a stored procedure.
    public const string GetPagedProcedureName = "usp_Identity_User_GetPaged";
}
