namespace Sugentra.ERP.UI.Models.Inventory;

public record QuarantineHold
{
    public long Id { get; set; }
    public long? BatchId { get; set; }
    public long ItemId { get; set; }
    public long WarehouseId { get; set; }
    public string HoldReason { get; set; } = "QcFailed"; // QcFailed | CustomerReturn | FumigationPending
    public string Status { get; set; } = "OnHold"; // OnHold | Released | Rejected
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReleasedAt { get; set; }
    public long? ReleasedBy { get; set; }
    public string? Notes { get; set; }
}
