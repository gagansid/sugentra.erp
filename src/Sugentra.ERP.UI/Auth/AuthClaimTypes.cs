namespace Sugentra.ERP.UI.Auth;

/// <summary>Custom claim types used to carry the API's JWT access/refresh tokens inside the UI's auth cookie.</summary>
public static class AuthClaimTypes
{
    public const string AccessToken = "access_token";
    public const string AccessTokenExpiresAt = "access_token_expires_at";
    public const string RefreshToken = "refresh_token";
    public const string Permission = "permission";
}
