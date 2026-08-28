namespace Sugentra.ERP.Api.Modules.Inventory.Dtos;

public record GoodsReceiptLineRequest(long ItemId, long BatchId, decimal Quantity, decimal UnitCost);

public record CreateGoodsReceiptRequest(
    long WarehouseId, string? VendorReference, DateTime ReceiptDate, string? Notes, List<GoodsReceiptLineRequest> Lines);

public record UpdateGoodsReceiptRequest(
    long WarehouseId, string? VendorReference, DateTime ReceiptDate, string? Notes, List<GoodsReceiptLineRequest> Lines);

public record GoodsReceiptLineResponse(long Id, long ItemId, long BatchId, decimal Quantity, decimal UnitCost);

public record GoodsReceiptResponse(
    long Id, string ReceiptNumber, long WarehouseId, string? VendorReference, DateTime ReceiptDate,
    string Status, string? CurrentApprovalLevel, string? Notes, DateTime CreatedAt, long? CreatedBy, string? CreatedByName,
    IReadOnlyList<GoodsReceiptLineResponse> Lines);
