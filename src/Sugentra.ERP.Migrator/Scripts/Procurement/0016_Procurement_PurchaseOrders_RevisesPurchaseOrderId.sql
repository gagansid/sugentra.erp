-- PO Revision/Amendment: single-level link from a revising PO back to the PO it supersedes.
-- See docs/modules/procurement.md "Suggested build order" point 4.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Procurement_PurchaseOrders') AND name = 'RevisesPurchaseOrderId')
BEGIN
    ALTER TABLE Procurement_PurchaseOrders ADD RevisesPurchaseOrderId BIGINT NULL;
    ALTER TABLE Procurement_PurchaseOrders ADD CONSTRAINT FK_Procurement_PurchaseOrders_RevisesPurchaseOrder
        FOREIGN KEY (RevisesPurchaseOrderId) REFERENCES Procurement_PurchaseOrders (Id);
END
GO
