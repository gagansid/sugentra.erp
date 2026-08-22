-- Setting_EmailAttachments: 1-to-many attachments per Setting_EmailHistory row.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_EmailAttachments')
BEGIN
    CREATE TABLE Setting_EmailAttachments
    (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_EmailAttachments PRIMARY KEY,
        EmailHistoryId  BIGINT         NOT NULL CONSTRAINT FK_Setting_EmailAttachments_EmailHistory
                                            REFERENCES Setting_EmailHistory (Id),
        FileName        NVARCHAR(300)  NOT NULL,
        ContentType     NVARCHAR(150)  NOT NULL,
        FileSize        BIGINT         NOT NULL,
        -- Path/URL to the stored file (disk/blob storage) — attachments are not stored as VARBINARY in SQL Server.
        StoragePath     NVARCHAR(500)  NOT NULL,

        CreatedAt       DATETIME2      NOT NULL CONSTRAINT DF_Setting_EmailAttachments_CreatedAt DEFAULT (GETDATE()),
        CreatedBy       BIGINT         NULL,
        UpdatedAt       DATETIME2      NULL,
        UpdatedBy       BIGINT         NULL,
        IsDeleted       BIT            NOT NULL CONSTRAINT DF_Setting_EmailAttachments_IsDeleted DEFAULT (0),
        DeletedAt       DATETIME2      NULL,
        DeletedBy       BIGINT         NULL
    );

    CREATE INDEX IX_Setting_EmailAttachments_EmailHistoryId ON Setting_EmailAttachments (EmailHistoryId);
END
