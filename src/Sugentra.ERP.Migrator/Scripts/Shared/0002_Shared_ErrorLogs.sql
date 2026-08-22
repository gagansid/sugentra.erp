-- Cross-cutting error log (API + UI) so unhandled exceptions are auditable instead of just written to console logs.
CREATE TABLE Shared_ErrorLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Source NVARCHAR(20) NOT NULL,           -- 'Api' or 'UI'
    OccurredAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    Endpoint NVARCHAR(500) NULL,            -- request path where the error happened
    HttpMethod NVARCHAR(10) NULL,
    StatusCode INT NULL,
    ExceptionType NVARCHAR(255) NULL,
    Message NVARCHAR(MAX) NULL,
    StackTrace NVARCHAR(MAX) NULL,
    QueryString NVARCHAR(1000) NULL,
    RequestParameters NVARCHAR(MAX) NULL,   -- route/query/body params, JSON-ish free text
    UserId BIGINT NULL,
    Username NVARCHAR(100) NULL,
    IpAddress NVARCHAR(45) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy BIGINT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy BIGINT NULL
);
GO

CREATE INDEX IX_Shared_ErrorLogs_OccurredAt ON Shared_ErrorLogs (OccurredAt DESC);
CREATE INDEX IX_Shared_ErrorLogs_Source ON Shared_ErrorLogs (Source);
GO
