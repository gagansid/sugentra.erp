using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Entities;

[Table("Inventory_StockOpnames")]
public class StockOpname : BaseAuditableEntity
{
    // Generated via DocumentNumberGeneratorService.GetNextNumberAsync("StockOpname"), never hand-entered.
    [Required, StringLength(50)]
    public string OpnameNumber { get; set; } = string.Empty;
    [Required]
    public long WarehouseId { get; set; }
    [Required]
    public DateTime OpnameDate { get; set; }
    [Required, StringLength(20)]
    public string Status { get; set; } = "Draft"; // Draft | WaitingApproval | Completed
    // Set alongside Status == "WaitingApproval"; the human-readable label is composed in the UI, not stored.
    [StringLength(100)]
    public string? CurrentApprovalLevel { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
}
