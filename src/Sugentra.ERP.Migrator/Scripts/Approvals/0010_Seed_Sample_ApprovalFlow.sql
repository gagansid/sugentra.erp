-- Sample approval flow so the "Approval Flows" and "My Approvals" screens have data to demo out of the box.
-- 2-level flow for GoodsReceipt: Level 1 = any Manager, Level 2 = any Admin (quorum: any-one-approver).
IF NOT EXISTS (SELECT 1 FROM Approval_FlowDefinitions WHERE DocumentType = 'GoodsReceipt' AND Name = 'Goods Receipt - Standard Approval')
BEGIN
    DECLARE @FlowId BIGINT;

    INSERT INTO Approval_FlowDefinitions (DocumentType, Name, MinAmount, MaxAmount, Priority, IsActive)
    VALUES ('GoodsReceipt', 'Goods Receipt - Standard Approval', 0, NULL, 0, 1);

    SET @FlowId = SCOPE_IDENTITY();

    DECLARE @Level1Id BIGINT, @Level2Id BIGINT;

    INSERT INTO Approval_FlowLevels (FlowDefinitionId, LevelNumber, Name, RequireAllApprovers)
    VALUES (@FlowId, 1, 'Manager Review', 0);
    SET @Level1Id = SCOPE_IDENTITY();

    INSERT INTO Approval_FlowLevels (FlowDefinitionId, LevelNumber, Name, RequireAllApprovers)
    VALUES (@FlowId, 2, 'Admin Approval', 0);
    SET @Level2Id = SCOPE_IDENTITY();

    INSERT INTO Approval_FlowLevelApprovers (FlowLevelId, RoleId)
    SELECT @Level1Id, Id FROM Identity_Roles WHERE Name = 'Manager';

    INSERT INTO Approval_FlowLevelApprovers (FlowLevelId, RoleId)
    SELECT @Level2Id, Id FROM Identity_Roles WHERE Name IN ('Admin', 'SuperAdmin');
END
GO
