namespace Sugentra.ERP.Api.Shared.Auth;

public class AccountLockoutOptions
{
    public const string SectionName = "AccountLockout";

    public int MaxFailedAttempts { get; set; } = 5;
}
