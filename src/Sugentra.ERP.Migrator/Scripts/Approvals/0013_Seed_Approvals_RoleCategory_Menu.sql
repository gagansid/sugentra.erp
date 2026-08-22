-- Sidebar menu entry for the Approval Role Categories admin screen, under the existing Approvals module.
DECLARE @ModuleId BIGINT = (SELECT Id FROM Setting_Modules WHERE Code = 'Approvals');

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT @ModuleId, NULL, 'Role Categories', 'ri-shield-user-line', 'ApprovalRoleCategories', 'Index', 'ApprovalRoleCategory_View', 30
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_Menus
    WHERE ModuleId = @ModuleId AND Controller = 'ApprovalRoleCategories' AND Action = 'Index'
);
GO
