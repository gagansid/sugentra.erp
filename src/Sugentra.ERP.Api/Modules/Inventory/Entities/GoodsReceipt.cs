using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Entities;

[Table("Inventory_GoodsReceipts")]
public class GoodsReceipt : BaseAuditableEntity
{
    // Generated via DocumentNumberGeneratorService.GetNextNumberAsync("GoodsReceipt"), never hand-entered.
    [Required, StringLength(50)]
    public string ReceiptNumber { get; set; } = string.Empty;
    [Required]
    public long WarehouseId { get; set; }
    [StringLength(200)]
    public string? VendorReference { get; set; }
    [Required]
    public DateTime ReceiptDate { get; set; }
    [Required, StringLength(20)]
    public string Status { get; set; } = "Draft"; // Draft | WaitingApproval | Posted
    // Set alongside Status == "WaitingApproval"; the human-readable label is composed in the UI, not stored.
    [StringLength(100)]
    public string? CurrentApprovalLevel { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
}
