-- Adds the GR<->PO link described in docs/modules/procurement.md's original v1 scope: nullable PurchaseOrderId
-- on Inventory_GoodsReceipts. When set, posting the receipt calls Procurement's IPurchaseOrderReceiptService
-- (see GoodsReceiptUseCase.FinalizePostAsync) so the referenced PO's ReceivedQuantity/LifecycleStatus updates,
-- without Inventory reading Procurement's tables directly.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Inventory_GoodsReceipts') AND name = 'PurchaseOrderId')
BEGIN
    ALTER TABLE Inventory_GoodsReceipts ADD PurchaseOrderId BIGINT NULL;
    ALTER TABLE Inventory_GoodsReceipts ADD CONSTRAINT FK_Inventory_GoodsReceipts_PurchaseOrder
        FOREIGN KEY (PurchaseOrderId) REFERENCES Procurement_PurchaseOrders (Id);
END
GO
