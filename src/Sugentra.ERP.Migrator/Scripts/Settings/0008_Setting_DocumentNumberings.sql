-- Setting_DocumentNumberings: per-document-type running number config, consumed via usp_Setting_DocumentNumbering_GetNext.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_DocumentNumberings')
BEGIN
    CREATE TABLE Setting_DocumentNumberings
    (
        Id             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_DocumentNumberings PRIMARY KEY,
        DocumentType   NVARCHAR(50)   NOT NULL,
        Prefix         NVARCHAR(20)   NULL,
        Suffix         NVARCHAR(20)   NULL,
        NumberLength   INT            NOT NULL CONSTRAINT DF_Setting_DocumentNumberings_NumberLength DEFAULT (6),
        ResetPeriod    NVARCHAR(20)   NOT NULL CONSTRAINT DF_Setting_DocumentNumberings_ResetPeriod DEFAULT ('Never'), -- Never | Yearly | Monthly
        CurrentNumber  INT            NOT NULL CONSTRAINT DF_Setting_DocumentNumberings_CurrentNumber DEFAULT (0),
        LastResetDate  DATETIME2      NULL,
        FormatTemplate NVARCHAR(100)  NULL, -- e.g. "{Prefix}/{Number}/{Suffix}/{Year}"

        CreatedAt      DATETIME2      NOT NULL CONSTRAINT DF_Setting_DocumentNumberings_CreatedAt DEFAULT (GETDATE()),
        CreatedBy      BIGINT         NULL,
        UpdatedAt      DATETIME2      NULL,
        UpdatedBy      BIGINT         NULL,
        IsDeleted      BIT            NOT NULL CONSTRAINT DF_Setting_DocumentNumberings_IsDeleted DEFAULT (0),
        DeletedAt      DATETIME2      NULL,
        DeletedBy      BIGINT         NULL,

        CONSTRAINT UQ_Setting_DocumentNumberings_DocumentType UNIQUE (DocumentType)
    );
END
