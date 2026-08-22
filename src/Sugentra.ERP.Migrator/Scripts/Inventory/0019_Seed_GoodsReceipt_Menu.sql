-- Sidebar menu entry for the new Goods Receipts screen.
DECLARE @ModuleId BIGINT = (SELECT Id FROM Setting_Modules WHERE Code = 'Inventory');

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT v.ModuleId, NULL, v.Name, v.Icon, v.Controller, v.Action, v.RequiredPermission, v.SortOrder
FROM (VALUES
    (@ModuleId, 'Goods Receipts', 'ri-truck-line', 'GoodsReceipts', 'Index', 'GoodsReceipt_View', 5)
) AS v(ModuleId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_Menus
    WHERE ModuleId = v.ModuleId AND Controller = v.Controller AND Action = v.Action
);
GO
