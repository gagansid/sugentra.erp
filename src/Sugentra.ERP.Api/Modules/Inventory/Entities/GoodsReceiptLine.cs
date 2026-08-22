using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Entities;

[Table("Inventory_GoodsReceiptLines")]
public class GoodsReceiptLine : BaseAuditableEntity
{
    [Required]
    public long ReceiptId { get; set; }
    [Required]
    public long ItemId { get; set; }
    [Required]
    public long BatchId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}
