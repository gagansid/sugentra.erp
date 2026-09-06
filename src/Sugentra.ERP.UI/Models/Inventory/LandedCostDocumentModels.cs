namespace Sugentra.ERP.UI.Models.Inventory;

public record LandedCostDocumentLineRequest(string CostType, decimal Amount, long CurrencyId, string? Notes);

public record CreateLandedCostDocumentRequest(
    long GoodsReceiptId, string AllocationMethod, string? Notes, List<LandedCostDocumentLineRequest> Lines);

public record LandedCostDocumentLineResponse(long Id, string CostType, decimal Amount, long CurrencyId, string? Notes);

public record LandedCostAllocationResultResponse(long Id, long BatchId, decimal Amount, long CurrencyId);

public record LandedCostDocumentResponse(
    long Id, string DocumentNumber, long GoodsReceiptId, string AllocationMethod, string Status, string? Notes,
    DateTime CreatedAt, IReadOnlyList<LandedCostDocumentLineResponse> Lines, IReadOnlyList<LandedCostAllocationResultResponse> Allocations);
