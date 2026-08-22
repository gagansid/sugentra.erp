namespace Sugentra.ERP.UI.Models.Inventory;

public record LandedCostAllocation
{
    public long Id { get; set; }
    public long BatchId { get; set; }
    public string CostType { get; set; } = "Freight"; // Freight | Insurance | Handling | Duty | Other
    public decimal Amount { get; set; }
    public long CurrencyId { get; set; }
    public string? Notes { get; set; }
}
