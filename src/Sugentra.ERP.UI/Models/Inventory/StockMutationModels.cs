namespace Sugentra.ERP.UI.Models.Inventory;

public record StockMutationLineRequest(long ItemId, long? BatchId, decimal Quantity);

public record CreateStockMutationRequest(
    string MutationType, long SourceWarehouseId, long? DestinationWarehouseId, string? VendorReference,
    DateTime MutationDate, string? Notes, List<StockMutationLineRequest> Lines);

public record UpdateStockMutationRequest(
    string MutationType, long SourceWarehouseId, long? DestinationWarehouseId, string? VendorReference,
    DateTime MutationDate, string? Notes, List<StockMutationLineRequest> Lines);

public record StockMutationLineResponse(long Id, long ItemId, long? BatchId, decimal Quantity);

public record StockMutationResponse(
    long Id, string MutationNumber, string MutationType, long SourceWarehouseId, long? DestinationWarehouseId,
    string? VendorReference, DateTime MutationDate, string Status, string? CurrentApprovalLevel, string? Notes,
    DateTime CreatedAt, long? CreatedBy, string? CreatedByName, IReadOnlyList<StockMutationLineResponse> Lines);
