namespace Sugentra.ERP.Api.Modules.Identity.Queries;

// SQL text only — no execution here. Repository executes these and returns results (Queries convention).
public static class UserRefreshTokenQuery
{
    public const string GetByTokenHashSql = "SELECT * FROM Identity_UserRefreshTokens WHERE TokenHash = @TokenHash AND IsDeleted = 0";

    public const string RevokeSql = "UPDATE Identity_UserRefreshTokens SET RevokedAt = GETDATE() WHERE Id = @Id";

    public const string GetByUserIdSql = "SELECT * FROM Identity_UserRefreshTokens WHERE UserId = @UserId AND IsDeleted = 0 ORDER BY CreatedAt DESC";
}
