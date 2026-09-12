-- Procurement module now has a working workspace (Purchase Requisitions, Purchase Orders).
UPDATE Setting_Modules SET Route = '/Workspace/Procurement' WHERE Code = 'Procurement' AND Route IS NULL;
GO
