using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Entities;

[Table("Inventory_LandedCostDocumentLines")]
public class LandedCostDocumentLine : BaseAuditableEntity
{
    [Required]
    public long LandedCostDocumentId { get; set; }
    [Required, StringLength(20)]
    public string CostType { get; set; } = string.Empty; // Freight | Insurance | Handling | Duty | Other
    public decimal Amount { get; set; }
    [Required]
    public long CurrencyId { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
}
