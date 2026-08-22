-- Approval_RequestLevels + Approval_RequestLevelApprovers: per-request snapshot of levels/eligible approvers,
-- copied from the flow definition at submission time so later flow edits never affect in-flight requests.
CREATE TABLE Approval_RequestLevels
(
    Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Approval_RequestLevels PRIMARY KEY,
    RequestId            BIGINT         NOT NULL,
    LevelNumber          INT            NOT NULL,
    Name                 NVARCHAR(150)  NOT NULL,
    RequireAllApprovers  BIT            NOT NULL,
    RequiredApproverCount INT           NOT NULL,
    ApprovedCount        INT            NOT NULL CONSTRAINT DF_Approval_RequestLevels_ApprovedCount DEFAULT (0),
    Status               NVARCHAR(20)   NOT NULL CONSTRAINT DF_Approval_RequestLevels_Status DEFAULT ('Pending'),

    CreatedAt            DATETIME2      NOT NULL CONSTRAINT DF_Approval_RequestLevels_CreatedAt DEFAULT (GETDATE()),
    CreatedBy            BIGINT         NULL,
    UpdatedAt            DATETIME2      NULL,
    UpdatedBy            BIGINT         NULL,
    IsDeleted            BIT            NOT NULL CONSTRAINT DF_Approval_RequestLevels_IsDeleted DEFAULT (0),
    DeletedAt            DATETIME2      NULL,
    DeletedBy            BIGINT         NULL,

    CONSTRAINT FK_Approval_RequestLevels_Request FOREIGN KEY (RequestId) REFERENCES Approval_Requests (Id)
);

CREATE INDEX IX_Approval_RequestLevels_RequestId ON Approval_RequestLevels (RequestId);
GO

CREATE TABLE Approval_RequestLevelApprovers
(
    Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Approval_RequestLevelApprovers PRIMARY KEY,
    RequestLevelId  BIGINT         NOT NULL,
    UserId          BIGINT         NOT NULL,
    HasActed        BIT            NOT NULL CONSTRAINT DF_Approval_RequestLevelApprovers_HasActed DEFAULT (0),

    CreatedAt       DATETIME2      NOT NULL CONSTRAINT DF_Approval_RequestLevelApprovers_CreatedAt DEFAULT (GETDATE()),
    CreatedBy       BIGINT         NULL,
    UpdatedAt       DATETIME2      NULL,
    UpdatedBy       BIGINT         NULL,
    IsDeleted       BIT            NOT NULL CONSTRAINT DF_Approval_RequestLevelApprovers_IsDeleted DEFAULT (0),
    DeletedAt       DATETIME2      NULL,
    DeletedBy       BIGINT         NULL,

    CONSTRAINT FK_Approval_RequestLevelApprovers_RequestLevel FOREIGN KEY (RequestLevelId) REFERENCES Approval_RequestLevels (Id),
    CONSTRAINT FK_Approval_RequestLevelApprovers_User FOREIGN KEY (UserId) REFERENCES Identity_Users (Id)
);

CREATE INDEX IX_Approval_RequestLevelApprovers_RequestLevelId ON Approval_RequestLevelApprovers (RequestLevelId);
CREATE INDEX IX_Approval_RequestLevelApprovers_UserId ON Approval_RequestLevelApprovers (UserId);
GO
