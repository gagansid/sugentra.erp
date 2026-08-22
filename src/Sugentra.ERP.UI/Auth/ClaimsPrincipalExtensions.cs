using System.Security.Claims;

namespace Sugentra.ERP.UI.Auth;

public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(this ClaimsPrincipal user, string code) =>
        user.HasClaim(AuthClaimTypes.Permission, code);
}
