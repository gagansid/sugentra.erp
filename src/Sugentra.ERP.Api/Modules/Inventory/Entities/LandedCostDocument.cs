using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Entities;

[Table("Inventory_LandedCostDocuments")]
public class LandedCostDocument : BaseAuditableEntity
{
    // Generated via DocumentNumberGeneratorService.GetNextNumberAsync("LandedCost"), never hand-entered.
    [Required, StringLength(50)]
    public string DocumentNumber { get; set; } = string.Empty;
    [Required]
    public long GoodsReceiptId { get; set; }
    [Required, StringLength(20)]
    public string AllocationMethod { get; set; } = "ByValue"; // ByValue | ByQuantity
    [Required, StringLength(20)]
    public string Status { get; set; } = "Draft"; // Draft | Posted
    [StringLength(500)]
    public string? Notes { get; set; }
}
