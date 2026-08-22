using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Approvals.Entities;

// Immutable append-only history entry — the full audit trail of who approved/rejected what, when, with what comment.
[Table("Approval_RequestActions")]
public class ApprovalRequestAction : BaseAuditableEntity
{
    public long RequestId { get; set; }
    public int LevelNumber { get; set; }
    public long ApproverUserId { get; set; }
    public string Action { get; set; } = string.Empty; // Approved / Rejected
    public string? Comment { get; set; }
    public DateTime ActionedAt { get; set; } = DateTime.UtcNow;
}
