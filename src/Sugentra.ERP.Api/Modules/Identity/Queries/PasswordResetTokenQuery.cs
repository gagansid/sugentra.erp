namespace Sugentra.ERP.Api.Modules.Identity.Queries;

public static class PasswordResetTokenQuery
{
    public const string GetActiveByUserAndCodeSql = """
        SELECT * FROM Identity_PasswordResetTokens
        WHERE UserId = @UserId AND Code = @Code AND IsUsed = 0 AND ExpiresAt > GETUTCDATE() AND IsDeleted = 0
        """;

    public const string MarkUsedSql = "UPDATE Identity_PasswordResetTokens SET IsUsed = 1, UsedAt = GETUTCDATE() WHERE Id = @Id";
}
