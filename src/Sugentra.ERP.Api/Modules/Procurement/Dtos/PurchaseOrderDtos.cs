namespace Sugentra.ERP.Api.Modules.Procurement.Dtos;

// Sources describe which PR (line) contributed how much quantity when this line was merged from multiple
// requisitions on the client; null PurchaseRequisitionId/PurchaseRequisitionLineId means manually added.
public record PurchaseOrderLineSourceRequest(long? PurchaseRequisitionId, long? PurchaseRequisitionLineId, decimal Quantity);

public record PurchaseOrderLineRequest(
    long ItemId, long WarehouseId, decimal Quantity, decimal UnitPrice, decimal DiscountPercent,
    List<PurchaseOrderLineSourceRequest>? Sources = null);

public record CreatePurchaseOrderRequest(
    List<long>? PurchaseRequisitionIds, long VendorId, long CurrencyId, int? PaymentTermDays, DateTime OrderDate,
    DateTime? ExpectedDeliveryDate, string? Notes, List<PurchaseOrderLineRequest> Lines);

// Update replaces the full line set — simplest correct behavior, avoids incremental add/remove diffing.
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


