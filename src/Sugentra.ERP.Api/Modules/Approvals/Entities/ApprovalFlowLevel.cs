using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Approvals.Entities;

// A tier ("jenjang") within a flow. RequireAllApprovers=true means every eligible approver snapshotted
// for this level must act before it completes (quorum); false means any one approver's action completes it.
[Table("Approval_FlowLevels")]
public class ApprovalFlowLevel : BaseAuditableEntity
{
    public long FlowDefinitionId { get; set; }
    public int LevelNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool RequireAllApprovers { get; set; } = true;
}
