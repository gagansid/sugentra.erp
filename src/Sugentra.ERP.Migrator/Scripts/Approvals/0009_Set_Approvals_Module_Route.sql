-- Give Approvals its own workspace landing page, same pattern as Inventory (0013_Set_Inventory_Module_Route.sql).
UPDATE Setting_Modules SET Route = '/Workspace/Approvals' WHERE Code = 'Approvals' AND Route IS NULL;
GO
