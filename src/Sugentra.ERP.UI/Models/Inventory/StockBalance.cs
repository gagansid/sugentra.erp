namespace Sugentra.ERP.UI.Models.Inventory;

public record StockBalance
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public long WarehouseId { get; set; }
    public long? BatchId { get; set; }
    public long UnitOfMeasurementId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityInQuarantine { get; set; }
    public decimal AverageCost { get; set; }
}
