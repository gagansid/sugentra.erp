using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;

namespace Sugentra.ERP.Api.Shared.Auth;

// The policy name itself IS the permission code, e.g. [Authorize(Policy = "User_Create")] — no per-permission
// policy registration is needed (see PermissionPolicyProvider), it's resolved dynamically at request time.
public class PermissionRequirement(string permissionCode) : IAuthorizationRequirement
{
    public string PermissionCode { get; } = permissionCode;
}

// Zero DB query on navigation: permissions were already resolved (role grants + allow/deny overrides,
// deny always wins) once at login and embedded as "permission" claims in the JWT — this handler only
// reads claims already present on the validated token, it never hits the database.
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasClaim("permission", requirement.PermissionCode))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

// Builds an ad-hoc AuthorizationPolicy for any policy name that isn't pre-registered, treating the
// policy name as a permission code. Lets controllers write [Authorize(Policy = "Role_Delete")] freely
// without a matching services.AddAuthorization(options => options.AddPolicy(...)) call for every permission.
public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider = new(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var existing = await _fallbackPolicyProvider.GetPolicyAsync(policyName);
        if (existing is not null)
        {
            return existing;
        }

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}
