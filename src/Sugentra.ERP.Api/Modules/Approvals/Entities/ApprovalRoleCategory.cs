using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Approvals.Entities;

// Pre-authorizes a Role to approve an approver-type category, enforced before a Flow can assign it.
[Table("Approval_RoleCategories")]
public class ApprovalRoleCategory : BaseAuditableEntity
{
    public long RoleId { get; set; }
    public string ApproverType { get; set; } = string.Empty;
    public string? Description { get; set; }
}
