-- Approval_RequestActions: immutable append-only history — the full "history approval" audit trail
-- (who approved/rejected what level, when, with what comment).
CREATE TABLE Approval_RequestActions
(
    Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Approval_RequestActions PRIMARY KEY,
    RequestId       BIGINT         NOT NULL,
    LevelNumber     INT            NOT NULL,
    ApproverUserId  BIGINT         NOT NULL,
    Action          NVARCHAR(20)   NOT NULL,
    Comment         NVARCHAR(1000) NULL,
    ActionedAt      DATETIME2      NOT NULL CONSTRAINT DF_Approval_RequestActions_ActionedAt DEFAULT (GETDATE()),

    CreatedAt       DATETIME2      NOT NULL CONSTRAINT DF_Approval_RequestActions_CreatedAt DEFAULT (GETDATE()),
    CreatedBy       BIGINT         NULL,
    UpdatedAt       DATETIME2      NULL,
    UpdatedBy       BIGINT         NULL,
    IsDeleted       BIT            NOT NULL CONSTRAINT DF_Approval_RequestActions_IsDeleted DEFAULT (0),
    DeletedAt       DATETIME2      NULL,
    DeletedBy       BIGINT         NULL,

    CONSTRAINT FK_Approval_RequestActions_Request FOREIGN KEY (RequestId) REFERENCES Approval_Requests (Id),
    CONSTRAINT FK_Approval_RequestActions_User FOREIGN KEY (ApproverUserId) REFERENCES Identity_Users (Id)
);

CREATE INDEX IX_Approval_RequestActions_RequestId ON Approval_RequestActions (RequestId);
GO
