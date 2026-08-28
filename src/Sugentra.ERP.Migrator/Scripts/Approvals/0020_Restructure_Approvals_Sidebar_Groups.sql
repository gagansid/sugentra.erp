-- Splits the Approvals module sidebar into Approvals / Settings / Reports sub-groups with menu-header
-- dividers, and renames "Approver Roles" to "Approval Types". Header dividers are plain Setting_Menus rows
-- with no Controller/Action (never permission-gated, rendered as a <li class="menu-header"> by _MenuNode.cshtml).
DECLARE @ModuleId BIGINT = (SELECT Id FROM Setting_Modules WHERE Code = 'Approvals');

UPDATE Setting_Menus
SET Name = 'Approval Types', SortOrder = 15
WHERE ModuleId = @ModuleId AND Controller = 'ApprovalRoleCategories' AND Action = 'Index' AND Name = 'Approver Roles';

UPDATE Setting_Menus
SET SortOrder = 10
WHERE ModuleId = @ModuleId AND Controller = 'MyApprovals' AND Action = 'Index';

UPDATE Setting_Menus
SET SortOrder = 20
WHERE ModuleId = @ModuleId AND Controller = 'ApprovalFlows' AND Action = 'Index';

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT @ModuleId, NULL, 'Approvals', NULL, NULL, NULL, NULL, 5
WHERE NOT EXISTS (SELECT 1 FROM Setting_Menus WHERE ModuleId = @ModuleId AND Name = 'Approvals' AND Controller IS NULL);

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT @ModuleId, NULL, 'Settings', NULL, NULL, NULL, NULL, 12
WHERE NOT EXISTS (SELECT 1 FROM Setting_Menus WHERE ModuleId = @ModuleId AND Name = 'Settings' AND Controller IS NULL);

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT @ModuleId, NULL, 'Reports', NULL, NULL, NULL, NULL, 30
WHERE NOT EXISTS (SELECT 1 FROM Setting_Menus WHERE ModuleId = @ModuleId AND Name = 'Reports' AND Controller IS NULL);
GO
