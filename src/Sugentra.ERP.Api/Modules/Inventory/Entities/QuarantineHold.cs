using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Entities;

[Table("Inventory_QuarantineHolds")]
public class QuarantineHold : BaseAuditableEntity
{
    public long? BatchId { get; set; }
    [Required]
    public long ItemId { get; set; }
    [Required]
    public long WarehouseId { get; set; }
    [Required, StringLength(30)]
    public string HoldReason { get; set; } = string.Empty; // QcFailed | CustomerReturn | FumigationPending
    [Required, StringLength(20)]
    public string Status { get; set; } = "OnHold"; // OnHold | Released | Rejected
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReleasedAt { get; set; }
    public long? ReleasedBy { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
}
