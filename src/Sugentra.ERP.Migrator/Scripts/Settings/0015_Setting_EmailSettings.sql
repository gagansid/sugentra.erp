-- Setting_EmailSettings: SMTP config, expected to hold a small number of rows with at most one IsActive = 1 at a time.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_EmailSettings')
BEGIN
    CREATE TABLE Setting_EmailSettings
    (
        Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_EmailSettings PRIMARY KEY,
        Name                NVARCHAR(150)  NOT NULL,
        Provider            NVARCHAR(50)   NOT NULL CONSTRAINT DF_Setting_EmailSettings_Provider DEFAULT ('Smtp'),
        Host                NVARCHAR(200)  NOT NULL,
        Port                INT            NOT NULL,
        -- MailKit SecureSocketOptions name: None / SslOnConnect / StartTls / StartTlsWhenAvailable.
        ConnectionSecurity  NVARCHAR(30)   NOT NULL CONSTRAINT DF_Setting_EmailSettings_ConnectionSecurity DEFAULT ('StartTls'),
        TimeoutSeconds      INT            NOT NULL CONSTRAINT DF_Setting_EmailSettings_TimeoutSeconds DEFAULT (30),
        Username            NVARCHAR(256)  NULL,
        Password            NVARCHAR(500)  NULL,
        IsActive            BIT            NOT NULL CONSTRAINT DF_Setting_EmailSettings_IsActive DEFAULT (1),
        LastTestedAt        DATETIME2      NULL,
        LastTestStatus      NVARCHAR(20)   NULL,
        LastTestMessage     NVARCHAR(1000) NULL,

        CreatedAt           DATETIME2      NOT NULL CONSTRAINT DF_Setting_EmailSettings_CreatedAt DEFAULT (GETDATE()),
        CreatedBy           BIGINT         NULL,
        UpdatedAt           DATETIME2      NULL,
        UpdatedBy           BIGINT         NULL,
        IsDeleted           BIT            NOT NULL CONSTRAINT DF_Setting_EmailSettings_IsDeleted DEFAULT (0),
        DeletedAt           DATETIME2      NULL,
        DeletedBy           BIGINT         NULL
    );
END

