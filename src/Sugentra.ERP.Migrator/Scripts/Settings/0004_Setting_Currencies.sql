-- Setting_Currencies: ISO currency codes used across Sales/Finance for multi-currency amounts.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_Currencies')
BEGIN
    CREATE TABLE Setting_Currencies
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_Currencies PRIMARY KEY,
        Code          NVARCHAR(3)    NOT NULL,
        Name          NVARCHAR(100)  NOT NULL,
        Symbol        NVARCHAR(10)   NULL,
        IsActive      BIT            NOT NULL CONSTRAINT DF_Setting_Currencies_IsActive DEFAULT (1),

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Setting_Currencies_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Setting_Currencies_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT UQ_Setting_Currencies_Code UNIQUE (Code)
    );
END
