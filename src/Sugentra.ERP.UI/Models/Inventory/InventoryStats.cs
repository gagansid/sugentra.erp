namespace Sugentra.ERP.UI.Models.Inventory;

public record MonthlyActivityPoint(string Month, int? GoodsReceipts, int? StockMutations, int? StockOpnames, int? QuarantineHolds);
public record StatusBreakdown(string Status, int Count);
public record StockBalanceSummary(decimal OnHand, decimal Reserved, decimal InQuarantine);

public record InventoryStats(
    int? TotalItems,
    int? TotalBatches,
    IReadOnlyList<StatusBreakdown>? GoodsReceipts,
    IReadOnlyList<StatusBreakdown>? QuarantineHolds,
    IReadOnlyList<StatusBreakdown>? StockMutations,
    IReadOnlyList<StatusBreakdown>? StockMutationTypes,
    IReadOnlyList<StatusBreakdown>? StockOpnames,
    StockBalanceSummary? StockBalances,
    int? TotalStockLedgerEntries,
    DateTime DateFrom,
    DateTime DateTo,
    IReadOnlyList<MonthlyActivityPoint> MonthlyActivity);

