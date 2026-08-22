-- Seed a sidebar menu entry for the Document Numbering list/edit page (was API-only until now).
DECLARE @ModuleId BIGINT = (SELECT Id FROM Setting_Modules WHERE Code = 'SystemAdministration');

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT @ModuleId, NULL, 'Document Numbering', 'ri-hashtag', 'DocumentNumberings', 'Index', 'DocumentNumbering_View', 70
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_Menus
    WHERE ModuleId = @ModuleId AND Controller = 'DocumentNumberings' AND Action = 'Index'
);
GO
