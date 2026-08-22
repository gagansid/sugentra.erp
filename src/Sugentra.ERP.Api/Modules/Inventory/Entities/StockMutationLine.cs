using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Entities;

[Table("Inventory_StockMutationLines")]
public class StockMutationLine : BaseAuditableEntity
{
    [Required]
    public long MutationId { get; set; }
    [Required]
    public long ItemId { get; set; }
    public long? BatchId { get; set; }
    public decimal Quantity { get; set; }
}
