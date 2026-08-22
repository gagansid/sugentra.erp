using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Entities;

[Table("Inventory_StockLedgers")]
public class StockLedger : BaseAuditableEntity
{
    [Required]
    public long ItemId { get; set; }
    [Required]
    public long WarehouseId { get; set; }
    public long? BatchId { get; set; }
    [Required, StringLength(20)]
    public string MovementType { get; set; } = string.Empty; // Receipt | Mutation | ToVendor | FromVendor | Consumption | Adjustment | Reservation | Release | Shipped
    public decimal QuantityChange { get; set; }
    public decimal UnitCost { get; set; }
    [StringLength(30)]
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    [Required]
    public DateTime MovementDate { get; set; }
}
