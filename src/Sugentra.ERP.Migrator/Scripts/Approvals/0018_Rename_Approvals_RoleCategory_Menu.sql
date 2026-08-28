-- Renamed sidebar label from "Role Categories" to "Approver Roles" to better match what the screen manages
-- (which Roles are pre-authorized approvers per Approver Type/DocumentType) — no schema/route change.
UPDATE Setting_Menus
SET Name = 'Approver Roles'
WHERE Controller = 'ApprovalRoleCategories' AND Action = 'Index' AND Name = 'Role Categories';
GO
