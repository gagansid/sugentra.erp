using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Entities;

[Table("Inventory_StockOpnameLines")]
public class StockOpnameLine : BaseAuditableEntity
{
    [Required]
    public long OpnameId { get; set; }
    [Required]
    public long ItemId { get; set; }
    public long? BatchId { get; set; }
    // Snapshot of Inventory_StockBalances.QuantityOnHand at count time.
    public decimal SystemQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal VarianceQuantity { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
}
