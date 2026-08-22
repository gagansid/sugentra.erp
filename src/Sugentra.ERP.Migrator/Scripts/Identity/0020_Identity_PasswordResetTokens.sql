-- Identity_PasswordResetTokens: self-service "forgot password" flow. Code is a 6-character
-- alphanumeric code, usable either typed manually or embedded in the emailed reset link.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Identity_PasswordResetTokens')
BEGIN
    CREATE TABLE Identity_PasswordResetTokens
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Identity_PasswordResetTokens PRIMARY KEY,
        UserId      BIGINT         NOT NULL CONSTRAINT FK_Identity_PasswordResetTokens_Users REFERENCES Identity_Users(Id),
        Code        CHAR(6)        NOT NULL,
        ExpiresAt   DATETIME2      NOT NULL,
        IsUsed      BIT            NOT NULL CONSTRAINT DF_Identity_PasswordResetTokens_IsUsed DEFAULT (0),
        UsedAt      DATETIME2      NULL,

        CreatedAt   DATETIME2      NOT NULL CONSTRAINT DF_Identity_PasswordResetTokens_CreatedAt DEFAULT (GETDATE()),
        CreatedBy   BIGINT         NULL,
        UpdatedAt   DATETIME2      NULL,
        UpdatedBy   BIGINT         NULL,
        IsDeleted   BIT            NOT NULL CONSTRAINT DF_Identity_PasswordResetTokens_IsDeleted DEFAULT (0),
        DeletedAt   DATETIME2      NULL,
        DeletedBy   BIGINT         NULL
    );

    CREATE INDEX IX_Identity_PasswordResetTokens_UserId_Code ON Identity_PasswordResetTokens(UserId, Code);
END
