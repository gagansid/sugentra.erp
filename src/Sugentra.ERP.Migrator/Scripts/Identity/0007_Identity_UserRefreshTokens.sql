-- Identity_UserRefreshTokens: supports JWT refresh flow (long-lived, revocable, hashed at rest)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Identity_UserRefreshTokens')
BEGIN
    CREATE TABLE Identity_UserRefreshTokens
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Identity_UserRefreshTokens PRIMARY KEY,
        UserId        BIGINT         NOT NULL,
        TokenHash     NVARCHAR(256)  NOT NULL,
        ExpiresAt     DATETIME2      NOT NULL,
        RevokedAt     DATETIME2      NULL,

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Identity_UserRefreshTokens_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Identity_UserRefreshTokens_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT FK_Identity_UserRefreshTokens_User FOREIGN KEY (UserId) REFERENCES Identity_Users (Id),
        CONSTRAINT UQ_Identity_UserRefreshTokens_TokenHash UNIQUE (TokenHash)
    );
END
