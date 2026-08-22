using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Approvals.Entities;

// Per-level progress snapshot for a request. RequiredApproverCount/RequireAllApprovers are copied from
// ApprovalFlowLevel at submission time so later flow edits never affect requests already in flight.
[Table("Approval_RequestLevels")]
public class ApprovalRequestLevel : BaseAuditableEntity
{
    public long RequestId { get; set; }
    public int LevelNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool RequireAllApprovers { get; set; }
    public int RequiredApproverCount { get; set; }
    public int ApprovedCount { get; set; }
    public string Status { get; set; } = "Pending";
}
