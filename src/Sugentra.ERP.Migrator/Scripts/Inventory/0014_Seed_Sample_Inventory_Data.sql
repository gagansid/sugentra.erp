-- Seed: sample Inventory data (Warehouses + Batches + Stock Mutations/Opnames + Quarantine Holds +
-- Stock Balances + Stock Ledgers) built on top of the existing MasterData_Items sample rows (ITM-0001..0006).
-- Warehouses aren't seeded anywhere yet, so this script adds them first before Inventory rows reference them.

INSERT INTO Setting_Warehouses (Code, Name, Address, City, Country, WarehouseType, CreatedBy)
SELECT v.Code, v.Name, v.Address, v.City, v.Country, v.WarehouseType, NULL
FROM (VALUES
    ('WH-MAIN', 'Main Warehouse',       'Jl. Industri Raya No. 1',  'Cirebon', 'Indonesia', 'Main'),
    ('WH-QC',   'Quarantine Warehouse', 'Jl. Industri Raya No. 1B', 'Cirebon', 'Indonesia', 'Quarantine'),
    ('WH-VDR',  'Vendor Consignment',   'Jl. Sungai Kahayan No. 8', 'Palangkaraya', 'Indonesia', 'Vendor')
) AS v(Code, Name, Address, City, Country, WarehouseType)
WHERE NOT EXISTS (SELECT 1 FROM Setting_Warehouses WHERE Code = v.Code);
GO

INSERT INTO Inventory_Batches (Code, ItemId, Grade, WarehouseId, ReceivedDate, LegalityDocumentType, LegalityDocumentNumber, SourceReference, CreatedBy)
SELECT v.Code, item.Id, v.Grade, wh.Id, v.ReceivedDate, v.LegalityDocumentType, v.LegalityDocumentNumber, v.SourceReference, NULL
FROM (VALUES
    ('BATCH-0001', 'ITM-0001', 'A',        'WH-MAIN', '2026-06-01', 'SVLK', 'SVLK-2026-0001', 'PO-2026-0011'),
    ('BATCH-0002', 'ITM-0001', 'A',        'WH-MAIN', '2026-07-10', 'SVLK', 'SVLK-2026-0002', 'PO-2026-0018'),
    ('BATCH-0003', 'ITM-0002', 'B',        'WH-MAIN', '2026-06-15', 'FSC',  'FSC-2026-0004',  'PO-2026-0013'),
    ('BATCH-0004', 'ITM-0003', 'Standard', 'WH-MAIN', '2026-07-01', NULL,   NULL,             'WO-2026-0007')
) AS v(Code, ItemCode, Grade, WarehouseCode, ReceivedDate, LegalityDocumentType, LegalityDocumentNumber, SourceReference)
JOIN MasterData_Items item ON item.Code = v.ItemCode
JOIN Setting_Warehouses wh ON wh.Code = v.WarehouseCode
WHERE NOT EXISTS (SELECT 1 FROM Inventory_Batches WHERE Code = v.Code);
GO

INSERT INTO Inventory_LandedCostAllocations (BatchId, CostType, Amount, CurrencyId, Notes, CreatedBy)
SELECT batch.Id, v.CostType, v.Amount, cur.Id, v.Notes, NULL
FROM (VALUES
    ('BATCH-0001', 'Freight',  1250000.00, 'IDR', 'Trucking Cirebon to warehouse'),
    ('BATCH-0001', 'Handling', 250000.00,  'IDR', 'Unloading + QC handling fee'),
    ('BATCH-0002', 'Freight',  1400000.00, 'IDR', 'Trucking Cirebon to warehouse')
) AS v(BatchCode, CostType, Amount, CurrencyCode, Notes)
JOIN Inventory_Batches batch ON batch.Code = v.BatchCode
JOIN Setting_Currencies cur ON cur.Code = v.CurrencyCode
WHERE NOT EXISTS (
    SELECT 1 FROM Inventory_LandedCostAllocations lca WHERE lca.BatchId = batch.Id AND lca.CostType = v.CostType
);
GO

INSERT INTO Inventory_QuarantineHolds (BatchId, ItemId, WarehouseId, HoldReason, Status, PlacedAt, Notes, CreatedBy)
SELECT batch.Id, item.Id, wh.Id, v.HoldReason, v.Status, v.PlacedAt, v.Notes, NULL
FROM (VALUES
    ('BATCH-0003', 'ITM-0002', 'WH-QC', 'QcFailed', 'OnHold', '2026-06-16', 'Moisture content above threshold, awaiting re-test.')
) AS v(BatchCode, ItemCode, WarehouseCode, HoldReason, Status, PlacedAt, Notes)
JOIN Inventory_Batches batch ON batch.Code = v.BatchCode
JOIN MasterData_Items item ON item.Code = v.ItemCode
JOIN Setting_Warehouses wh ON wh.Code = v.WarehouseCode
WHERE NOT EXISTS (
    SELECT 1 FROM Inventory_QuarantineHolds q WHERE q.BatchId = batch.Id AND q.HoldReason = v.HoldReason
);
GO

-- Stock mutation header + lines: uses a fixed sample number since it predates any real Document Numbering
-- sequence call (usp_Setting_DocumentNumbering_GetNext is only invoked from the API's CreateAsync flow).
INSERT INTO Inventory_StockMutations (MutationNumber, MutationType, SourceWarehouseId, DestinationWarehouseId, VendorReference, MutationDate, Status, Notes, CreatedBy)
SELECT v.MutationNumber, v.MutationType, src.Id, dst.Id, v.VendorReference, v.MutationDate, v.Status, v.Notes, NULL
FROM (VALUES
    ('MUT/2026/0001', 'Internal', 'WH-MAIN', 'WH-QC', NULL, '2026-06-16', 'Completed', 'Move suspect batch to quarantine warehouse for re-testing.')
) AS v(MutationNumber, MutationType, SourceWarehouseCode, DestinationWarehouseCode, VendorReference, MutationDate, Status, Notes)
JOIN Setting_Warehouses src ON src.Code = v.SourceWarehouseCode
JOIN Setting_Warehouses dst ON dst.Code = v.DestinationWarehouseCode
WHERE NOT EXISTS (SELECT 1 FROM Inventory_StockMutations WHERE MutationNumber = v.MutationNumber);
GO

INSERT INTO Inventory_StockMutationLines (MutationId, ItemId, BatchId, Quantity, CreatedBy)
SELECT mut.Id, item.Id, batch.Id, v.Quantity, NULL
FROM (VALUES
    ('MUT/2026/0001', 'ITM-0002', 'BATCH-0003', 500.0000)
) AS v(MutationNumber, ItemCode, BatchCode, Quantity)
JOIN Inventory_StockMutations mut ON mut.MutationNumber = v.MutationNumber
JOIN MasterData_Items item ON item.Code = v.ItemCode
JOIN Inventory_Batches batch ON batch.Code = v.BatchCode
WHERE NOT EXISTS (
    SELECT 1 FROM Inventory_StockMutationLines l WHERE l.MutationId = mut.Id AND l.ItemId = item.Id AND l.BatchId = batch.Id
);
GO

INSERT INTO Inventory_StockOpnames (OpnameNumber, WarehouseId, OpnameDate, Status, Notes, CreatedBy)
SELECT v.OpnameNumber, wh.Id, v.OpnameDate, v.Status, v.Notes, NULL
FROM (VALUES
    ('OPN/202607/0001', 'WH-MAIN', '2026-07-31', 'Approved', 'Monthly cycle count for July 2026.')
) AS v(OpnameNumber, WarehouseCode, OpnameDate, Status, Notes)
JOIN Setting_Warehouses wh ON wh.Code = v.WarehouseCode
WHERE NOT EXISTS (SELECT 1 FROM Inventory_StockOpnames WHERE OpnameNumber = v.OpnameNumber);
GO

INSERT INTO Inventory_StockOpnameLines (OpnameId, ItemId, BatchId, SystemQuantity, CountedQuantity, VarianceQuantity, Notes, CreatedBy)
SELECT opname.Id, item.Id, batch.Id, v.SystemQuantity, v.CountedQuantity, v.CountedQuantity - v.SystemQuantity, v.Notes, NULL
FROM (VALUES
    ('OPN/202607/0001', 'ITM-0001', 'BATCH-0001', 2000.0000, 1985.5000, 'Minor shrinkage from drying.'),
    ('OPN/202607/0001', 'ITM-0001', 'BATCH-0002', 1500.0000, 1500.0000, NULL),
    ('OPN/202607/0001', 'ITM-0003', 'BATCH-0004', 300.0000,  298.0000,  'Two sheets damaged, written off separately.')
) AS v(OpnameNumber, ItemCode, BatchCode, SystemQuantity, CountedQuantity, Notes)
JOIN Inventory_StockOpnames opname ON opname.OpnameNumber = v.OpnameNumber
JOIN MasterData_Items item ON item.Code = v.ItemCode
JOIN Inventory_Batches batch ON batch.Code = v.BatchCode
WHERE NOT EXISTS (
    SELECT 1 FROM Inventory_StockOpnameLines l WHERE l.OpnameId = opname.Id AND l.ItemId = item.Id AND l.BatchId = batch.Id
);
GO

INSERT INTO Inventory_StockBalances (ItemId, WarehouseId, BatchId, UnitOfMeasurementId, QuantityOnHand, QuantityReserved, QuantityInQuarantine, AverageCost, CreatedBy)
SELECT item.Id, wh.Id, batch.Id, uom.Id, v.QuantityOnHand, v.QuantityReserved, v.QuantityInQuarantine, v.AverageCost, NULL
FROM (VALUES
    ('ITM-0001', 'WH-MAIN', 'BATCH-0001', 'KG',  1985.5000, 0.0000, 0.0000, 25625.00),
    ('ITM-0001', 'WH-MAIN', 'BATCH-0002', 'KG',  1500.0000, 0.0000, 0.0000, 25933.33),
    ('ITM-0002', 'WH-QC',   'BATCH-0003', 'KG',  0.0000,    0.0000, 500.0000, 18000.00),
    ('ITM-0003', 'WH-MAIN', 'BATCH-0004', 'PCS', 298.0000,  0.0000, 0.0000, 45000.00)
) AS v(ItemCode, WarehouseCode, BatchCode, UomCode, QuantityOnHand, QuantityReserved, QuantityInQuarantine, AverageCost)
JOIN MasterData_Items item ON item.Code = v.ItemCode
JOIN Setting_Warehouses wh ON wh.Code = v.WarehouseCode
JOIN Inventory_Batches batch ON batch.Code = v.BatchCode
JOIN Setting_UnitsOfMeasurement uom ON uom.Code = v.UomCode
WHERE NOT EXISTS (
    SELECT 1 FROM Inventory_StockBalances b WHERE b.ItemId = item.Id AND b.WarehouseId = wh.Id AND b.BatchId = batch.Id
);
GO

INSERT INTO Inventory_StockLedgers (ItemId, WarehouseId, BatchId, MovementType, QuantityChange, UnitCost, ReferenceType, ReferenceId, MovementDate, CreatedBy)
SELECT item.Id, wh.Id, batch.Id, v.MovementType, v.QuantityChange, v.UnitCost, v.ReferenceType, ref.Id, v.MovementDate, NULL
FROM (VALUES
    ('ITM-0001', 'WH-MAIN', 'BATCH-0001', 'Receipt',  2000.0000,  25625.00, NULL,             NULL,               '2026-06-01'),
    ('ITM-0001', 'WH-MAIN', 'BATCH-0002', 'Receipt',  1500.0000,  25933.33, NULL,             NULL,               '2026-07-10'),
    ('ITM-0002', 'WH-MAIN', 'BATCH-0003', 'Receipt',  500.0000,   18000.00, NULL,             NULL,               '2026-06-15'),
    ('ITM-0003', 'WH-MAIN', 'BATCH-0004', 'Receipt',  300.0000,   45000.00, NULL,             NULL,               '2026-07-01'),
    ('ITM-0002', 'WH-QC',   'BATCH-0003', 'Mutation', 500.0000,   18000.00, 'StockMutation',   'MUT/2026/0001',    '2026-06-16'),
    ('ITM-0001', 'WH-MAIN', 'BATCH-0001', 'Adjustment', -14.5000, 25625.00, 'StockOpname',     'OPN/202607/0001',  '2026-07-31'),
    ('ITM-0003', 'WH-MAIN', 'BATCH-0004', 'Adjustment', -2.0000,  45000.00, 'StockOpname',     'OPN/202607/0001',  '2026-07-31')
) AS v(ItemCode, WarehouseCode, BatchCode, MovementType, QuantityChange, UnitCost, ReferenceType, ReferenceCode, MovementDate)
JOIN MasterData_Items item ON item.Code = v.ItemCode
JOIN Setting_Warehouses wh ON wh.Code = v.WarehouseCode
JOIN Inventory_Batches batch ON batch.Code = v.BatchCode
OUTER APPLY (
    SELECT mut.Id FROM Inventory_StockMutations mut WHERE v.ReferenceType = 'StockMutation' AND mut.MutationNumber = v.ReferenceCode
    UNION ALL
    SELECT opn.Id FROM Inventory_StockOpnames opn WHERE v.ReferenceType = 'StockOpname' AND opn.OpnameNumber = v.ReferenceCode
) ref
WHERE NOT EXISTS (
    SELECT 1 FROM Inventory_StockLedgers l
    WHERE l.ItemId = item.Id AND l.WarehouseId = wh.Id AND l.BatchId = batch.Id
      AND l.MovementType = v.MovementType AND l.MovementDate = v.MovementDate
);
GO
