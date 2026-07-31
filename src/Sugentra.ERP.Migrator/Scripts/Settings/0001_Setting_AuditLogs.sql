-- Setting_AuditLogs: system-wide audit trail, written by Shared/Logging/AuditLogService for every module.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_AuditLogs')
BEGIN
    CREATE TABLE Setting_AuditLogs
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_AuditLogs PRIMARY KEY,
        TableName     NVARCHAR(128)  NOT NULL,
        RecordId      BIGINT         NOT NULL,
        Action        NVARCHAR(20)   NOT NULL,
        OldValues     NVARCHAR(MAX)  NULL,
        NewValues     NVARCHAR(MAX)  NULL,
        ChangedBy     BIGINT         NOT NULL,
        ChangedAt     DATETIME2      NOT NULL CONSTRAINT DF_Setting_AuditLogs_ChangedAt DEFAULT (SYSUTCDATETIME()),

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Setting_AuditLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Setting_AuditLogs_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,
        RowVersion    ROWVERSION     NOT NULL,

        CONSTRAINT FK_Setting_AuditLogs_ChangedBy FOREIGN KEY (ChangedBy) REFERENCES Identity_Users (Id)
    );
END
