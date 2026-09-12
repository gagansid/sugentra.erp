namespace Sugentra.ERP.UI.Models.Procurement;

public record PurchaseOrderLineSourceRequest(long? PurchaseRequisitionId, long? PurchaseRequisitionLineId, decimal Quantity);

public record PurchaseOrderLineRequest(
    long ItemId, long WarehouseId, decimal Quantity, decimal UnitPrice, decimal DiscountPercent,
    List<PurchaseOrderLineSourceRequest>? Sources = null);

public record CreatePurchaseOrderRequest(
    List<long>? PurchaseRequisitionIds, long VendorId, long CurrencyId, int? PaymentTermDays, DateTime OrderDate,
    DateTime? ExpectedDeliveryDate, string? Notes, List<PurchaseOrderLineRequest> Lines);

public record UpdatePurchaseOrderRequest(
    long VendorId, long CurrencyId, int? PaymentTermDays, DateTime OrderDate,
    DateTime? ExpectedDeliveryDate, string? Notes, List<PurchaseOrderLineRequest> Lines);

public record PurchaseOrderLineSourceResponse(long? PurchaseRequisitionId, string? RequisitionNumber, decimal Quantity);

public record PurchaseOrderLineResponse(
    long Id, long ItemId, string? ItemCode, string? ItemName, long WarehouseId,
    decimal Quantity, decimal UnitPrice, decimal DiscountPercent, decimal ReceivedQuantity,
    IReadOnlyList<PurchaseOrderLineSourceResponse> Sources);

public record PurchaseOrderSourceRequisitionResponse(long Id, string RequisitionNumber);

public record PurchaseOrderResponse(
    long Id, string OrderNumber, IReadOnlyList<PurchaseOrderSourceRequisitionResponse> SourceRequisitions, long VendorId, string? VendorCode, string? VendorName,
    long CurrencyId, int? PaymentTermDays, DateTime OrderDate, DateTime? ExpectedDeliveryDate,
    string Status, string? CurrentApprovalLevel, string? Notes,
    DateTime CreatedAt, long? CreatedBy, string? CreatedByName, IReadOnlyList<PurchaseOrderLineResponse> Lines);

public record PurchaseOrderAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);
