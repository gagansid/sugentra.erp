-- Module registry row + sidebar menu entries for the dedicated Approvals menu (flow config + inbox).
INSERT INTO Setting_Modules (Code, Name, Icon, Route, SortOrder)
SELECT 'Approvals', 'Approvals', 'ri-checkbox-multiple-line', NULL, 45
WHERE NOT EXISTS (SELECT 1 FROM Setting_Modules WHERE Code = 'Approvals');
GO

DECLARE @ModuleId BIGINT = (SELECT Id FROM Setting_Modules WHERE Code = 'Approvals');

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT v.ModuleId, NULL, v.Name, v.Icon, v.Controller, v.Action, v.RequiredPermission, v.SortOrder
FROM (VALUES
    (@ModuleId, 'My Approvals',    'ri-inbox-line',       'MyApprovals',   'Index', 'ApprovalRequest_View', 10),
    (@ModuleId, 'Approval Flows',  'ri-git-branch-line',  'ApprovalFlows', 'Index', 'ApprovalFlow_View',    20)
) AS v(ModuleId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_Menus
    WHERE ModuleId = v.ModuleId AND Controller = v.Controller AND Action = v.Action
);
GO
