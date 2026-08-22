namespace Sugentra.ERP.Api.Shared.Contracts;

// Cross-module read-only lookup so non-Identity modules (e.g. Approvals) can resolve role-based approvers
// without referencing Identity's namespace directly (module isolation rule in AGENTS.md).
public interface IRoleMembershipService
{
    Task<IReadOnlyList<long>> GetUserIdsInRoleAsync(long roleId);
}
