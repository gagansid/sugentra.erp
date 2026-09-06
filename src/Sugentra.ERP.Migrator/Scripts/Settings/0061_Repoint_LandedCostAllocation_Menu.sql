-- Landed Cost workflow's entry point is now the LandedCostDocuments controller (Draft->Posted document,
-- not a manual per-batch allocation form) -- repoint the existing sidebar menu row instead of adding a duplicate.
UPDATE Setting_Menus
SET Controller = 'LandedCostDocuments', Name = 'Landed Cost Documents'
WHERE Controller = 'LandedCostAllocations' AND Action = 'Index';
