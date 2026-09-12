-- Sidebar menu entries for the Procurement module's list pages.
DECLARE @ModuleId BIGINT = (SELECT Id FROM Setting_Modules WHERE Code = 'Procurement');

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT v.ModuleId, NULL, v.Name, v.Icon, v.Controller, v.Action, v.RequiredPermission, v.SortOrder
FROM (VALUES
    (@ModuleId, 'Purchase Requisitions', 'ri-file-list-3-line',    'PurchaseRequisitions', 'Index', 'PurchaseRequisition_View', 10),
    (@ModuleId, 'Purchase Orders',       'ri-shopping-cart-2-line', 'PurchaseOrders',       'Index', 'PurchaseOrder_View',       20)
) AS v(ModuleId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_Menus
    WHERE ModuleId = v.ModuleId AND Controller = v.Controller AND Action = v.Action
);
GO
