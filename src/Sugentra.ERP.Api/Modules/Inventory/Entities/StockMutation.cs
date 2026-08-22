using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Entities;

[Table("Inventory_StockMutations")]
public class StockMutation : BaseAuditableEntity
{
    // Generated via DocumentNumberGeneratorService.GetNextNumberAsync("StockMutation"), never hand-entered.
    [Required, StringLength(50)]
    public string MutationNumber { get; set; } = string.Empty;
    [Required, StringLength(20)]
    public string MutationType { get; set; } = string.Empty; // Internal | ToVendor | FromVendor
    [Required]
    public long SourceWarehouseId { get; set; }
    public long? DestinationWarehouseId { get; set; }
    [StringLength(200)]
    public string? VendorReference { get; set; }
    [Required]
    public DateTime MutationDate { get; set; }
    [Required, StringLength(20)]
    public string Status { get; set; } = "Draft"; // Draft | Approved | Completed
    [StringLength(500)]
    public string? Notes { get; set; }
}
