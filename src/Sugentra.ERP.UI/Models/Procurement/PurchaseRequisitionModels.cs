namespace Sugentra.ERP.UI.Models.Procurement;

public record PurchaseRequisitionLineRequest(long ItemId, decimal Quantity, string? Notes);

public record CreatePurchaseRequisitionRequest(
    long WarehouseId, DateTime RequisitionDate, string? Notes, List<PurchaseRequisitionLineRequest> Lines);

public record UpdatePurchaseRequisitionRequest(
    long WarehouseId, DateTime RequisitionDate, string? Notes, List<PurchaseRequisitionLineRequest> Lines);

public record PurchaseRequisitionLineResponse(long Id, long ItemId, string? ItemCode, string? ItemName, decimal Quantity, string? Notes, decimal OrderedQuantity);

public record PurchaseRequisitionOrderResponse(long Id, string OrderNumber);

public record PurchaseRequisitionResponse(
    long Id, string RequisitionNumber, long RequesterUserId, string? RequesterName, long WarehouseId,
    DateTime RequisitionDate, string Status, string? CurrentApprovalLevel, string? Notes,
    DateTime CreatedAt, long? CreatedBy, string? CreatedByName, IReadOnlyList<PurchaseRequisitionLineResponse> Lines,
    IReadOnlyList<PurchaseRequisitionOrderResponse> LinkedOrders, string OrderStatus);

public record PurchaseRequisitionAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);
