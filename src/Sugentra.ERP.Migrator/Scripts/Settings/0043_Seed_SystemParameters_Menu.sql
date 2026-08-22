-- Seed a sidebar menu entry for the System Parameters list/edit page (was API-only until now).
DECLARE @ModuleId BIGINT = (SELECT Id FROM Setting_Modules WHERE Code = 'SystemAdministration');

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT @ModuleId, NULL, 'System Parameters', 'ri-sliders-line', 'SystemParameters', 'Index', 'SystemParameter_View', 60
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_Menus
    WHERE ModuleId = @ModuleId AND Controller = 'SystemParameters' AND Action = 'Index'
);
GO
