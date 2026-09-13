using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Procurement.Entities;

[Table("Procurement_PurchaseOrders")]
public class PurchaseOrder : BaseAuditableEntity
{
    // Generated via IDocumentNumberGeneratorService.GetNextAsync("PurchaseOrder"), never hand-entered.
    [Required, StringLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    [Required]
    public long VendorId { get; set; }
    [Required]
    public long CurrencyId { get; set; }
    public int? PaymentTermDays { get; set; }
    [Required]
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    [Required, StringLength(20)]
    public string Status { get; set; } = "Draft"; // Draft | WaitingApproval | Approved | Rejected | PartiallyReceived | FullyReceived | Closed
    // Second, independent status axis (only meaningful once Status == "Approved"); null before that.
    [StringLength(20)]
    public string? LifecycleStatus { get; set; } // Open | PartiallyReceived | FullyReceived | Cancelled | Closed | Superseded
    // Set alongside Status == "WaitingApproval"; the human-readable label is composed in the UI, not stored.
    [StringLength(100)]
    public string? CurrentApprovalLevel { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
    // Single-level link: set on the NEW PO created by a "Revise" action; the OLD PO gets LifecycleStatus = "Superseded".
    public long? RevisesPurchaseOrderId { get; set; }
}
