using Dapper;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Inventory.Queries;

public record InventoryStatsDto(
    int TotalBatches,
    int QuarantineHoldsOnHold, int QuarantineHoldsReleased, int QuarantineHoldsRejected,
    int StockOpnamesDraft, int StockOpnamesWaitingApproval, int StockOpnamesCompleted,
    int StockMutationsDraft, int StockMutationsWaitingApproval, int StockMutationsCompleted,
    int StockMutationsInternal, int StockMutationsToVendor, int StockMutationsFromVendor,
    int GoodsReceiptsDraft, int GoodsReceiptsWaitingApproval, int GoodsReceiptsPosted,
    decimal StockBalanceOnHand, decimal StockBalanceReserved, decimal StockBalanceInQuarantine,
    int TotalStockLedgerEntries);
public record MonthlyCountDto(string MonthKey, int Count);

/// <summary>Read path for the Inventory workspace dashboard cards/chart — raw Dapper counts, no business rules.</summary>
public class InventoryStatsQuery(IDbConnectionFactory connectionFactory)
{
    private const string Sql = """
        SELECT (SELECT COUNT(*) FROM Inventory_Batches WHERE IsDeleted = 0) AS TotalBatches,
               (SELECT COUNT(*) FROM Inventory_QuarantineHolds WHERE IsDeleted = 0 AND Status = 'OnHold') AS QuarantineHoldsOnHold,
               (SELECT COUNT(*) FROM Inventory_QuarantineHolds WHERE IsDeleted = 0 AND Status = 'Released') AS QuarantineHoldsReleased,
               (SELECT COUNT(*) FROM Inventory_QuarantineHolds WHERE IsDeleted = 0 AND Status = 'Rejected') AS QuarantineHoldsRejected,
               (SELECT COUNT(*) FROM Inventory_StockOpnames WHERE IsDeleted = 0 AND Status = 'Draft') AS StockOpnamesDraft,
               (SELECT COUNT(*) FROM Inventory_StockOpnames WHERE IsDeleted = 0 AND Status = 'WaitingApproval') AS StockOpnamesWaitingApproval,
               (SELECT COUNT(*) FROM Inventory_StockOpnames WHERE IsDeleted = 0 AND Status = 'Completed') AS StockOpnamesCompleted,
               (SELECT COUNT(*) FROM Inventory_StockMutations WHERE IsDeleted = 0 AND Status = 'Draft') AS StockMutationsDraft,
               (SELECT COUNT(*) FROM Inventory_StockMutations WHERE IsDeleted = 0 AND Status = 'WaitingApproval') AS StockMutationsWaitingApproval,
               (SELECT COUNT(*) FROM Inventory_StockMutations WHERE IsDeleted = 0 AND Status = 'Completed') AS StockMutationsCompleted,
               (SELECT COUNT(*) FROM Inventory_StockMutations WHERE IsDeleted = 0 AND MutationType = 'Internal') AS StockMutationsInternal,
               (SELECT COUNT(*) FROM Inventory_StockMutations WHERE IsDeleted = 0 AND MutationType = 'ToVendor') AS StockMutationsToVendor,
               (SELECT COUNT(*) FROM Inventory_StockMutations WHERE IsDeleted = 0 AND MutationType = 'FromVendor') AS StockMutationsFromVendor,
               (SELECT COUNT(*) FROM Inventory_GoodsReceipts WHERE IsDeleted = 0 AND Status = 'Draft') AS GoodsReceiptsDraft,
               (SELECT COUNT(*) FROM Inventory_GoodsReceipts WHERE IsDeleted = 0 AND Status = 'WaitingApproval') AS GoodsReceiptsWaitingApproval,
               (SELECT COUNT(*) FROM Inventory_GoodsReceipts WHERE IsDeleted = 0 AND Status = 'Posted') AS GoodsReceiptsPosted,
               (SELECT ISNULL(SUM(QuantityOnHand), 0) FROM Inventory_StockBalances WHERE IsDeleted = 0) AS StockBalanceOnHand,
               (SELECT ISNULL(SUM(QuantityReserved), 0) FROM Inventory_StockBalances WHERE IsDeleted = 0) AS StockBalanceReserved,
               (SELECT ISNULL(SUM(QuantityInQuarantine), 0) FROM Inventory_StockBalances WHERE IsDeleted = 0) AS StockBalanceInQuarantine,
               (SELECT COUNT(*) FROM Inventory_StockLedgers WHERE IsDeleted = 0) AS TotalStockLedgerEntries
        """;

    public async Task<InventoryStatsDto> GetAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        return (await connection.QuerySingleAsync<InventoryStatsDto>(Sql, new { }))!;
    }

    public Task<IReadOnlyList<MonthlyCountDto>> GetGoodsReceiptMonthlyCountsAsync(DateTime from, DateTime toExclusive) =>
        GetMonthlyCountsAsync("Inventory_GoodsReceipts", "ReceiptDate", from, toExclusive);

    public Task<IReadOnlyList<MonthlyCountDto>> GetStockMutationMonthlyCountsAsync(DateTime from, DateTime toExclusive) =>
        GetMonthlyCountsAsync("Inventory_StockMutations", "MutationDate", from, toExclusive);

    public Task<IReadOnlyList<MonthlyCountDto>> GetStockOpnameMonthlyCountsAsync(DateTime from, DateTime toExclusive) =>
        GetMonthlyCountsAsync("Inventory_StockOpnames", "OpnameDate", from, toExclusive);

    public Task<IReadOnlyList<MonthlyCountDto>> GetQuarantineHoldMonthlyCountsAsync(DateTime from, DateTime toExclusive) =>
        GetMonthlyCountsAsync("Inventory_QuarantineHolds", "PlacedAt", from, toExclusive);

    // Table/column names below are fixed, developer-controlled constants (never user input), so string
    // interpolation here carries no SQL-injection risk despite not being parameterized like @From/@ToExclusive.
    private async Task<IReadOnlyList<MonthlyCountDto>> GetMonthlyCountsAsync(string table, string dateColumn, DateTime from, DateTime toExclusive)
    {
        var sql = $"""
            SELECT FORMAT({dateColumn}, 'yyyy-MM') AS MonthKey, COUNT(*) AS Count
            FROM {table}
            WHERE IsDeleted = 0 AND {dateColumn} >= @From AND {dateColumn} < @ToExclusive
            GROUP BY FORMAT({dateColumn}, 'yyyy-MM')
            """;
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryListAsync<MonthlyCountDto>(sql, new { From = from, ToExclusive = toExclusive });
    }
}

