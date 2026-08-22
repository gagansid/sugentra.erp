-- Seed a sidebar menu entry for the Company Profile edit page (was API-only until now).
DECLARE @ModuleId BIGINT = (SELECT Id FROM Setting_Modules WHERE Code = 'Settings');

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT @ModuleId, NULL, 'Company Profile', 'ri-building-line', 'CompanyProfile', 'Index', 'CompanyProfile_View', 5
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_Menus
    WHERE ModuleId = @ModuleId AND Controller = 'CompanyProfile' AND Action = 'Index'
);
GO
