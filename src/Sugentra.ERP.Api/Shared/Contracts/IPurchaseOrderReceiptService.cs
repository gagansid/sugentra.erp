namespace Sugentra.ERP.Api.Shared.Contracts;

// Implemented by Procurement, called by Inventory after posting a Goods Receipt that references a PurchaseOrderId,
// so Procurement can update the receiving PO's line ReceivedQuantity/status without Inventory reading Procurement's tables.
public record PurchaseOrderReceiptLineUpdate(long PurchaseOrderLineId, decimal ReceivedQuantity);

public interface IPurchaseOrderReceiptService
{
    Task ApplyReceiptAsync(long purchaseOrderId, IReadOnlyList<PurchaseOrderReceiptLineUpdate> lines);
}
