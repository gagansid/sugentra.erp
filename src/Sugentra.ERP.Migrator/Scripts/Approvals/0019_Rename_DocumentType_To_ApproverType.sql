-- "DocumentType" on Approval_FlowDefinitions/Approval_RoleCategories was ambiguous against
-- Setting_DocumentNumberings.DocumentType (a completely different concept). Renamed to ApproverType
-- to reflect what it actually represents: the approver category a Role/Flow applies to.
EXEC sp_rename 'Approval_FlowDefinitions.DocumentType', 'ApproverType', 'COLUMN';
GO

DROP INDEX IX_Approval_FlowDefinitions_DocumentType ON Approval_FlowDefinitions;
GO

CREATE INDEX IX_Approval_FlowDefinitions_ApproverType ON Approval_FlowDefinitions (ApproverType);
GO

EXEC sp_rename 'Approval_RoleCategories.DocumentType', 'ApproverType', 'COLUMN';
GO

DROP INDEX IX_Approval_RoleCategories_DocumentType ON Approval_RoleCategories;
GO

CREATE INDEX IX_Approval_RoleCategories_ApproverType ON Approval_RoleCategories (ApproverType);
GO

DROP INDEX UX_Approval_RoleCategories_Role_DocumentType_Active ON Approval_RoleCategories;
GO

CREATE UNIQUE INDEX UX_Approval_RoleCategories_Role_ApproverType_Active
    ON Approval_RoleCategories (RoleId, ApproverType)
    WHERE IsDeleted = 0;
GO
