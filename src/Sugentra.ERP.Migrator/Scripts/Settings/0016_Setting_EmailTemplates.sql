-- Setting_EmailTemplates: Code is the lookup key callers pass to IEmailService.SendAsync (e.g. "PASSWORD_RESET").
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_EmailTemplates')
BEGIN
    CREATE TABLE Setting_EmailTemplates
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_EmailTemplates PRIMARY KEY,
        Code          NVARCHAR(100)  NOT NULL,
        Name          NVARCHAR(200)  NOT NULL,
        Description         NVARCHAR(500)  NULL,
        FromEmail     NVARCHAR(256)  NOT NULL,
        FromName      NVARCHAR(200)  NULL,
        ReplyToEmail        NVARCHAR(256)  NULL,
        IsReplyToEnabled    BIT            NOT NULL CONSTRAINT DF_Setting_EmailTemplates_IsReplyToEnabled DEFAULT (0),
        Subject       NVARCHAR(300)  NOT NULL,
        -- Supports {{PlaceholderName}} tokens, replaced by IEmailService before sending.
        BodyHtml      NVARCHAR(MAX)  NOT NULL,
        -- Plain-text fallback for clients that don't render HTML.
        BodyText            NVARCHAR(MAX)  NULL,
        -- Comma-separated list of valid placeholder names for this template, e.g. "UserName,ResetLink,ExpiryMinutes".
        AvailablePlaceholders NVARCHAR(500) NULL,
        IsActive      BIT            NOT NULL CONSTRAINT DF_Setting_EmailTemplates_IsActive DEFAULT (1),

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Setting_EmailTemplates_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Setting_EmailTemplates_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT UQ_Setting_EmailTemplates_Code UNIQUE (Code)
    );
END
