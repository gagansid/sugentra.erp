-- Setting_EmailHistory: append-only send log. Stores a snapshot of the rendered Subject/BodyHtml/From (not just the
-- TemplateCode/EmailSettings FK), so history stays accurate even if the template or SMTP config changes later.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_EmailHistory')
BEGIN
    CREATE TABLE Setting_EmailHistory
    (
        Id                 BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_EmailHistory PRIMARY KEY,
        FromEmail          NVARCHAR(256)  NOT NULL,
        FromName           NVARCHAR(200)  NULL,
        ToEmail            NVARCHAR(256)  NOT NULL,
        CcEmail            NVARCHAR(500)  NULL,
        BccEmail           NVARCHAR(500)  NULL,
        Subject            NVARCHAR(300)  NOT NULL,
        BodyHtml           NVARCHAR(MAX)  NOT NULL,
        -- No FK to Setting_EmailTemplates.Code on purpose — history must survive template rename/delete.
        TemplateCode       NVARCHAR(100)  NULL,
        -- What business feature triggered this send (e.g. "Identity"/"PasswordReset") and which record (e.g. UserId),
        -- so a specific send can be traced back to its source even when the same TemplateCode is reused elsewhere.
        SourceModule       NVARCHAR(100)  NULL,
        SourceReferenceId  BIGINT         NULL,
        Status             NVARCHAR(20)   NOT NULL,
        ErrorMessage       NVARCHAR(1000) NULL,
        RetryCount         INT            NOT NULL CONSTRAINT DF_Setting_EmailHistory_RetryCount DEFAULT (0),
        AttachmentCount    INT            NOT NULL CONSTRAINT DF_Setting_EmailHistory_AttachmentCount DEFAULT (0),
        SentAt             DATETIME2      NOT NULL CONSTRAINT DF_Setting_EmailHistory_SentAt DEFAULT (GETDATE()),

        CreatedAt          DATETIME2      NOT NULL CONSTRAINT DF_Setting_EmailHistory_CreatedAt DEFAULT (GETDATE()),
        CreatedBy          BIGINT         NULL,
        UpdatedAt          DATETIME2      NULL,
        UpdatedBy          BIGINT         NULL,
        IsDeleted          BIT            NOT NULL CONSTRAINT DF_Setting_EmailHistory_IsDeleted DEFAULT (0),
        DeletedAt          DATETIME2      NULL,
        DeletedBy          BIGINT         NULL
    );

    CREATE INDEX IX_Setting_EmailHistory_ToEmail ON Setting_EmailHistory (ToEmail);
    CREATE INDEX IX_Setting_EmailHistory_SentAt ON Setting_EmailHistory (SentAt);
    CREATE INDEX IX_Setting_EmailHistory_SourceModule ON Setting_EmailHistory (SourceModule, SourceReferenceId);
END

