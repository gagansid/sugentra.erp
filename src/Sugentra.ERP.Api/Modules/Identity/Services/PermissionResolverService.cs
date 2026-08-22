using Sugentra.ERP.Api.Modules.Identity.Repositories;

namespace Sugentra.ERP.Api.Modules.Identity.Services;

// Permission Resolution Algorithm (see plan.md): role grants ∪ allow-overrides − deny-overrides. Deny always wins.
public class PermissionResolverService(
    IUserRoleRepository userRoleRepository,
    IRolePermissionRepository rolePermissionRepository,
    IUserPermissionRepository userPermissionRepository)
{
    public async Task<IReadOnlyList<string>> ResolveAsync(long userId)
    {
        var roleIds = await userRoleRepository.GetRoleIdsByUserIdAsync(userId);
        var basePermissions = await rolePermissionRepository.GetPermissionCodesByRoleIdsAsync(roleIds);
        var overrides = await userPermissionRepository.GetOverridesByUserIdAsync(userId);

        var permissions = new HashSet<string>(basePermissions);

        // Two passes (not a single ordered loop) so Deny always wins regardless of the order rows come back from the DB.
        foreach (var permissionOverride in overrides.Where(o => o.IsAllowed))
        {
            permissions.Add(permissionOverride.Code);
        }

        foreach (var permissionOverride in overrides.Where(o => !o.IsAllowed))
        {
            permissions.Remove(permissionOverride.Code);
        }

        return permissions.ToList();
    }
}
