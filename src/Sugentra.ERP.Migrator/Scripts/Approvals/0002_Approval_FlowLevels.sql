-- Approval_FlowLevels: tiers ("jenjang") within a flow. RequireAllApprovers = quorum (every eligible
-- approver must act) vs any-one-approver completes the level.
CREATE TABLE Approval_FlowLevels
(
    Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Approval_FlowLevels PRIMARY KEY,
    FlowDefinitionId    BIGINT         NOT NULL,
    LevelNumber         INT            NOT NULL,
    Name                NVARCHAR(150)  NOT NULL,
    RequireAllApprovers BIT            NOT NULL CONSTRAINT DF_Approval_FlowLevels_RequireAllApprovers DEFAULT (1),

    CreatedAt           DATETIME2      NOT NULL CONSTRAINT DF_Approval_FlowLevels_CreatedAt DEFAULT (GETDATE()),
    CreatedBy           BIGINT         NULL,
    UpdatedAt           DATETIME2      NULL,
    UpdatedBy           BIGINT         NULL,
    IsDeleted           BIT            NOT NULL CONSTRAINT DF_Approval_FlowLevels_IsDeleted DEFAULT (0),
    DeletedAt           DATETIME2      NULL,
    DeletedBy           BIGINT         NULL,

    CONSTRAINT FK_Approval_FlowLevels_FlowDefinition FOREIGN KEY (FlowDefinitionId) REFERENCES Approval_FlowDefinitions (Id)
);

CREATE INDEX IX_Approval_FlowLevels_FlowDefinitionId ON Approval_FlowLevels (FlowDefinitionId);
GO
