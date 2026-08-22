-- Sidebar menu entry for the new Warehouses management screen (permission already seeded in 0036).
INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT mo.Id, NULL, 'Warehouses', 'ri-building-line', 'Warehouses', 'Index', 'Warehouse_View', 30
FROM Setting_Modules mo
WHERE mo.Code = 'Settings'
  AND NOT EXISTS (
      SELECT 1 FROM Setting_Menus me
      WHERE me.ModuleId = mo.Id AND me.Controller = 'Warehouses' AND me.Action = 'Index'
  );
GO
