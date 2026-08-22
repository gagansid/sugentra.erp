-- Sidebar menu entries for the Inventory module's list pages.
DECLARE @ModuleId BIGINT = (SELECT Id FROM Setting_Modules WHERE Code = 'Inventory');

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT v.ModuleId, NULL, v.Name, v.Icon, v.Controller, v.Action, v.RequiredPermission, v.SortOrder
FROM (VALUES
    (@ModuleId, 'Batches',           'ri-barcode-box-line',  'Batches',          'Index', 'Batch_View',           10),
    (@ModuleId, 'Stock Mutations',   'ri-arrow-left-right-line', 'StockMutations', 'Index', 'StockMutation_View',   20),
    (@ModuleId, 'Stock Opnames',     'ri-clipboard-line',    'StockOpnames',     'Index', 'StockOpname_View',     30),
    (@ModuleId, 'Quarantine Holds',  'ri-shield-cross-line', 'QuarantineHolds',  'Index', 'QuarantineHold_View',  40),
    (@ModuleId, 'Stock Balances',    'ri-stack-line',        'StockBalances',    'Index', 'StockBalance_View',    50),
    (@ModuleId, 'Stock Ledgers',     'ri-file-list-3-line',  'StockLedgers',     'Index', 'StockLedger_View',     60)
) AS v(ModuleId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_Menus
    WHERE ModuleId = v.ModuleId AND Controller = v.Controller AND Action = v.Action
);
GO
