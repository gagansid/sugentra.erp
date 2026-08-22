-- Approval_FlowLevelApprovers: eligible-approver definition for a level — either "anyone holding RoleId"
-- or a specific UserId. At least one of RoleId/UserId must be set.
CREATE TABLE Approval_FlowLevelApprovers
(
    Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Approval_FlowLevelApprovers PRIMARY KEY,
    FlowLevelId BIGINT         NOT NULL,
    RoleId      BIGINT         NULL,
    UserId      BIGINT         NULL,

    CreatedAt   DATETIME2      NOT NULL CONSTRAINT DF_Approval_FlowLevelApprovers_CreatedAt DEFAULT (GETDATE()),
    CreatedBy   BIGINT         NULL,
    UpdatedAt   DATETIME2      NULL,
    UpdatedBy   BIGINT         NULL,
    IsDeleted   BIT            NOT NULL CONSTRAINT DF_Approval_FlowLevelApprovers_IsDeleted DEFAULT (0),
    DeletedAt   DATETIME2      NULL,
    DeletedBy   BIGINT         NULL,

    CONSTRAINT FK_Approval_FlowLevelApprovers_FlowLevel FOREIGN KEY (FlowLevelId) REFERENCES Approval_FlowLevels (Id),
    CONSTRAINT FK_Approval_FlowLevelApprovers_Role FOREIGN KEY (RoleId) REFERENCES Identity_Roles (Id),
    CONSTRAINT FK_Approval_FlowLevelApprovers_User FOREIGN KEY (UserId) REFERENCES Identity_Users (Id),
    CONSTRAINT CK_Approval_FlowLevelApprovers_RoleOrUser CHECK (RoleId IS NOT NULL OR UserId IS NOT NULL)
);

CREATE INDEX IX_Approval_FlowLevelApprovers_FlowLevelId ON Approval_FlowLevelApprovers (FlowLevelId);
GO
