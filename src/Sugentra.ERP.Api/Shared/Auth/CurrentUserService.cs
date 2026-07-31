using System.Security.Claims;

namespace Sugentra.ERP.Api.Shared.Auth;

public interface ICurrentUserService
{
    long? UserId { get; }
    string? Username { get; }
}

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public long? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Username => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
}
