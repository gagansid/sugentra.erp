-- Full reset of Procurement transactional data (child tables first for FK safety) plus identity reseed,
-- so all document numbers restart at 1. Master data (Vendors/Currencies) is untouched.
DELETE FROM Procurement_PurchaseOrderLineSources;
GO
DELETE FROM Procurement_PurchaseOrderRequisitions;
GO
DELETE FROM Procurement_PurchaseOrderLines;
GO
DELETE FROM Procurement_PurchaseOrders;
GO
DELETE FROM Procurement_PurchaseRequisitionLines;
GO
DELETE FROM Procurement_PurchaseRequisitions;
GO

DBCC CHECKIDENT ('Procurement_PurchaseOrderLineSources', RESEED, 0);
DBCC CHECKIDENT ('Procurement_PurchaseOrderRequisitions', RESEED, 0);
DBCC CHECKIDENT ('Procurement_PurchaseOrderLines', RESEED, 0);
DBCC CHECKIDENT ('Procurement_PurchaseOrders', RESEED, 0);
DBCC CHECKIDENT ('Procurement_PurchaseRequisitionLines', RESEED, 0);
DBCC CHECKIDENT ('Procurement_PurchaseRequisitions', RESEED, 0);
GO
