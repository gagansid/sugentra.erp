-- Fix menu order: Goods Receipts must come after Batches (a receipt line requires an existing batch).
UPDATE Setting_Menus
SET SortOrder = 15
WHERE Controller = 'GoodsReceipts' AND Action = 'Index';
GO
