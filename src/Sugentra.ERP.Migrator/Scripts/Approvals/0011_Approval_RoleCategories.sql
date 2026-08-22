-- Approval_RoleCategories: pre-authorizes a Role to approve a given document-type category, enforced by
-- ApprovalFlowUseCase.ValidateAsync before a Flow level can assign that Role to an approval level.
CREATE TABLE Approval_RoleCategories
(
    Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Approval_RoleCategories PRIMARY KEY,
    RoleId       BIGINT         NOT NULL,
    DocumentType NVARCHAR(50)   NOT NULL,

    CreatedAt    DATETIME2      NOT NULL CONSTRAINT DF_Approval_RoleCategories_CreatedAt DEFAULT (GETDATE()),
    CreatedBy    BIGINT         NULL,
    UpdatedAt    DATETIME2      NULL,
    UpdatedBy    BIGINT         NULL,
    IsDeleted    BIT            NOT NULL CONSTRAINT DF_Approval_RoleCategories_IsDeleted DEFAULT (0),
    DeletedAt    DATETIME2      NULL,
    DeletedBy    BIGINT         NULL,

    CONSTRAINT FK_Approval_RoleCategories_Role FOREIGN KEY (RoleId) REFERENCES Identity_Roles (Id),
    CONSTRAINT UQ_Approval_RoleCategories_Role_DocumentType UNIQUE (RoleId, DocumentType)
);

CREATE INDEX IX_Approval_RoleCategories_DocumentType ON Approval_RoleCategories (DocumentType);
GO
