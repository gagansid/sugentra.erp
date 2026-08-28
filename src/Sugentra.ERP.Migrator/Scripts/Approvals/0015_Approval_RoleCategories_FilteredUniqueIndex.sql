-- The original UQ_Approval_RoleCategories_Role_DocumentType constraint applied to ALL rows, including
-- soft-deleted ones, so re-adding a (RoleId, DocumentType) pair after it was soft-deleted would fail with
-- a UNIQUE KEY violation. Replace it with a filtered unique index that only enforces uniqueness among
-- active (IsDeleted = 0) rows.
ALTER TABLE Approval_RoleCategories DROP CONSTRAINT UQ_Approval_RoleCategories_Role_DocumentType;
GO

CREATE UNIQUE INDEX UX_Approval_RoleCategories_Role_DocumentType_Active
    ON Approval_RoleCategories (RoleId, DocumentType)
    WHERE IsDeleted = 0;
GO
