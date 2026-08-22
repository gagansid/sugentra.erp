using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Approvals.Entities;

// Eligible-approver definition for a level: either "anyone holding RoleId" or a specific UserId.
[Table("Approval_FlowLevelApprovers")]
public class ApprovalFlowLevelApprover : BaseAuditableEntity
{
    public long FlowLevelId { get; set; }
    public long? RoleId { get; set; }
    public long? UserId { get; set; }
}
