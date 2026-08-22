using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Entities;

[Table("Inventory_StockBalances")]
public class StockBalance : BaseAuditableEntity
{
    [Required]
    public long ItemId { get; set; }
    [Required]
    public long WarehouseId { get; set; }
    public long? BatchId { get; set; }
    [Required]
    public long UnitOfMeasurementId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityInQuarantine { get; set; }
    // Weighted-average unit cost, recomputed on each Receipt.
    public decimal AverageCost { get; set; }
}
