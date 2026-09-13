-- Clears approval history tied to Procurement/Inventory documents ahead of a full data reset for those
-- modules, so stale Approval_Requests don't end up pointing at unrelated documents once ids restart at 1.
DELETE a
FROM Approval_RequestActions a
INNER JOIN Approval_Requests r ON r.Id = a.RequestId
WHERE r.DocumentType IN ('PurchaseRequisition', 'PurchaseOrder', 'GoodsReceipt', 'StockMutation', 'StockOpname', 'LandedCost');
GO

DELETE la
FROM Approval_RequestLevelApprovers la
INNER JOIN Approval_RequestLevels l ON l.Id = la.RequestLevelId
INNER JOIN Approval_Requests r ON r.Id = l.RequestId
WHERE r.DocumentType IN ('PurchaseRequisition', 'PurchaseOrder', 'GoodsReceipt', 'StockMutation', 'StockOpname', 'LandedCost');
GO

DELETE l
FROM Approval_RequestLevels l
INNER JOIN Approval_Requests r ON r.Id = l.RequestId
WHERE r.DocumentType IN ('PurchaseRequisition', 'PurchaseOrder', 'GoodsReceipt', 'StockMutation', 'StockOpname', 'LandedCost');
GO

DELETE FROM Approval_Requests
WHERE DocumentType IN ('PurchaseRequisition', 'PurchaseOrder', 'GoodsReceipt', 'StockMutation', 'StockOpname', 'LandedCost');
GO
