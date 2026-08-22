using Sugentra.ERP.Api.Modules.Identity.Repositories;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Identity.Services;

public class RoleMembershipService(IUserRoleRepository userRoleRepository) : IRoleMembershipService
{
    public Task<IReadOnlyList<long>> GetUserIdsInRoleAsync(long roleId) => userRoleRepository.GetUserIdsByRoleIdAsync(roleId);
}
