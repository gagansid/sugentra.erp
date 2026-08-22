using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Approvals.Entities;

// Snapshot of who was eligible to act on a request level at submission time (role membership resolved once).
[Table("Approval_RequestLevelApprovers")]
public class ApprovalRequestLevelApprover : BaseAuditableEntity
{
    public long RequestLevelId { get; set; }
    public long UserId { get; set; }
    public bool HasActed { get; set; }
}
