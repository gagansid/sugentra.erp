namespace Sugentra.ERP.UI.Models.Inventory;

public record StockLedger
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public long WarehouseId { get; set; }
    public long? BatchId { get; set; }
    public string MovementType { get; set; } = string.Empty; // Receipt | Mutation | ToVendor | FromVendor | Consumption | Adjustment | Reservation | Release | Shipped
    public decimal QuantityChange { get; set; }
    public decimal UnitCost { get; set; }
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public DateTime MovementDate { get; set; }
}
