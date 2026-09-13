-- Resets the running number back to 0 for Procurement/Inventory document types, to pair with the full
-- transactional data wipe in those modules so the next document created starts numbering at 1 again.
UPDATE Setting_DocumentNumberings
SET CurrentNumber = 0
WHERE DocumentType IN ('PurchaseRequisition', 'PurchaseOrder', 'GoodsReceipt', 'StockMutation', 'StockOpname', 'LandedCost');
GO
