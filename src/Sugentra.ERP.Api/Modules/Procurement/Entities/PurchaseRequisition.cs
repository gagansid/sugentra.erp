using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Procurement.Entities;

[Table("Procurement_PurchaseRequisitions")]
public class PurchaseRequisition : BaseAuditableEntity
{
    // Generated via IDocumentNumberGeneratorService.GetNextAsync("PurchaseRequisition"), never hand-entered.
    [Required, StringLength(50)]
    public string RequisitionNumber { get; set; } = string.Empty;
    [Required]
    public long RequesterUserId { get; set; }
    [Required]
    public long WarehouseId { get; set; }
    [Required]
    public DateTime RequisitionDate { get; set; }
    [Required, StringLength(20)]
    public string Status { get; set; } = "Draft"; // Draft | WaitingApproval | Approved | Rejected | Closed
    // Set alongside Status == "WaitingApproval"; the human-readable label is composed in the UI, not stored.
    [StringLength(100)]
    public string? CurrentApprovalLevel { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
}
