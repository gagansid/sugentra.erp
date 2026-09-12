using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Approvals.Entities;

// One instance of a document going through approval. Status: Pending / Approved / Rejected / Cancelled.
[Table("Approval_Requests")]
public class ApprovalRequest : BaseAuditableEntity
{
    public string DocumentType { get; set; } = string.Empty;
    public long DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public long? FlowDefinitionId { get; set; }
    public decimal? Amount { get; set; }
    public long? CurrencyId { get; set; }
    public long? WarehouseId { get; set; }
    public string Status { get; set; } = "Pending";
    public int CurrentLevelNumber { get; set; } = 1;
    public long RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
