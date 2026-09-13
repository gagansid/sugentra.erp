namespace Sugentra.ERP.Api.Shared.Contracts;

// Implemented by Procurement, called by Inventory after posting a Goods Receipt that references a PurchaseOrderId,
// so Procurement can update the receiving PO's line ReceivedQuantity/status without Inventory reading Procurement's tables.
// Keyed by ItemId (not PurchaseOrderLineId) - Inventory only knows the item/qty it received, not Procurement's line ids.
public record PurchaseOrderReceiptItemUpdate(long ItemId, decimal ReceivedQuantity);

public interface IPurchaseOrderReceiptService
{
    Task ApplyReceiptAsync(long purchaseOrderId, IReadOnlyList<PurchaseOrderReceiptItemUpdate> items);
}

