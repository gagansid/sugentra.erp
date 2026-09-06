using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Inventory.Queries;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Inventory.Controllers;

public record MonthlyActivityPoint(string Month, int? GoodsReceipts, int? StockMutations, int? StockOpnames, int? QuarantineHolds);
public record StatusBreakdown(string Status, int Count);
public record StockBalanceSummary(decimal OnHand, decimal Reserved, decimal InQuarantine);

public record InventoryStatsResponse(
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

[Route("api/inventory/stats")]
[Authorize]
public class InventoryStatsController(InventoryStatsQuery statsQuery, IItemDirectoryService itemDirectoryService) : ApiControllerBase
{
    // Each card/series is only populated if the caller holds the matching *_View permission — same claim
    // check PermissionAuthorizationHandler uses, applied per-field instead of gating the whole endpoint.
    [HttpGet]
    public async Task<IActionResult> GetStats([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var stats = await statsQuery.GetAsync();

        int? totalItems = User.HasClaim("permission", "MasterData_View")
            ? await itemDirectoryService.GetActiveCountAsync()
            : null;
        int? totalBatches = User.HasClaim("permission", "Batch_View") ? stats.TotalBatches : null;

        IReadOnlyList<StatusBreakdown>? goodsReceiptStatuses = User.HasClaim("permission", "GoodsReceipt_View")
            ?
            [
                new StatusBreakdown("Draft", stats.GoodsReceiptsDraft),
                new StatusBreakdown("Waiting Approval", stats.GoodsReceiptsWaitingApproval),
                new StatusBreakdown("Posted", stats.GoodsReceiptsPosted)
            ]
            : null;
        IReadOnlyList<StatusBreakdown>? quarantineHoldStatuses = User.HasClaim("permission", "QuarantineHold_View")
            ?
            [
                new StatusBreakdown("On Hold", stats.QuarantineHoldsOnHold),
                new StatusBreakdown("Released", stats.QuarantineHoldsReleased),
                new StatusBreakdown("Rejected", stats.QuarantineHoldsRejected)
            ]
            : null;
        IReadOnlyList<StatusBreakdown>? stockMutationStatuses = User.HasClaim("permission", "StockMutation_View")
            ?
            [
                new StatusBreakdown("Draft", stats.StockMutationsDraft),
                new StatusBreakdown("Waiting Approval", stats.StockMutationsWaitingApproval),
                new StatusBreakdown("Completed", stats.StockMutationsCompleted)
            ]
            : null;
        IReadOnlyList<StatusBreakdown>? stockMutationTypes = User.HasClaim("permission", "StockMutation_View")
            ?
            [
                new StatusBreakdown("Internal", stats.StockMutationsInternal),
                new StatusBreakdown("To Vendor", stats.StockMutationsToVendor),
                new StatusBreakdown("From Vendor", stats.StockMutationsFromVendor)
            ]
            : null;
        IReadOnlyList<StatusBreakdown>? stockOpnameStatuses = User.HasClaim("permission", "StockOpname_View")
            ?
            [
                new StatusBreakdown("Draft", stats.StockOpnamesDraft),
                new StatusBreakdown("Waiting Approval", stats.StockOpnamesWaitingApproval),
                new StatusBreakdown("Completed", stats.StockOpnamesCompleted)
            ]
            : null;
        StockBalanceSummary? stockBalances = User.HasClaim("permission", "StockBalance_View")
            ? new StockBalanceSummary(stats.StockBalanceOnHand, stats.StockBalanceReserved, stats.StockBalanceInQuarantine)
            : null;
        int? totalStockLedgerEntries = User.HasClaim("permission", "StockLedger_View") ? stats.TotalStockLedgerEntries : null;

        // Default the chart window to the current calendar month (1st to last day) when no filter is given.
        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var from = dateFrom?.Date ?? monthStart;
        var to = dateTo?.Date ?? monthEnd;
        var toExclusive = to.AddDays(1);

        var months = new List<string>();
        for (var cursor = new DateTime(from.Year, from.Month, 1); cursor < toExclusive; cursor = cursor.AddMonths(1))
        {
            months.Add(cursor.ToString("yyyy-MM"));
        }

        var goodsReceiptCounts = User.HasClaim("permission", "GoodsReceipt_View")
            ? (await statsQuery.GetGoodsReceiptMonthlyCountsAsync(from, toExclusive)).ToDictionary(m => m.MonthKey, m => m.Count)
            : null;
        var stockMutationCounts = User.HasClaim("permission", "StockMutation_View")
            ? (await statsQuery.GetStockMutationMonthlyCountsAsync(from, toExclusive)).ToDictionary(m => m.MonthKey, m => m.Count)
            : null;
        var stockOpnameCounts = User.HasClaim("permission", "StockOpname_View")
            ? (await statsQuery.GetStockOpnameMonthlyCountsAsync(from, toExclusive)).ToDictionary(m => m.MonthKey, m => m.Count)
            : null;
        var quarantineHoldCounts = User.HasClaim("permission", "QuarantineHold_View")
            ? (await statsQuery.GetQuarantineHoldMonthlyCountsAsync(from, toExclusive)).ToDictionary(m => m.MonthKey, m => m.Count)
            : null;

        var monthlyActivity = months
            .Select(month => new MonthlyActivityPoint(
                month,
                goodsReceiptCounts?.GetValueOrDefault(month, 0),
                stockMutationCounts?.GetValueOrDefault(month, 0),
                stockOpnameCounts?.GetValueOrDefault(month, 0),
                quarantineHoldCounts?.GetValueOrDefault(month, 0)))
            .ToList();

        return Success(new InventoryStatsResponse(
            totalItems, totalBatches, goodsReceiptStatuses, quarantineHoldStatuses, stockMutationStatuses, stockMutationTypes,
            stockOpnameStatuses, stockBalances, totalStockLedgerEntries, from, to, monthlyActivity));
    }
}

