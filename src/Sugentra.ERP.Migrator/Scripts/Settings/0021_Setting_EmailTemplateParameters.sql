CREATE TABLE Setting_EmailTemplateParameters
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    EmailTemplateId BIGINT NOT NULL FOREIGN KEY REFERENCES Setting_EmailTemplates(Id),
    ParamKey NVARCHAR(100) NOT NULL,
    ParamValue NVARCHAR(500) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy BIGINT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy BIGINT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy BIGINT NULL
);
GO

CREATE UNIQUE INDEX IX_Setting_EmailTemplateParameters_TemplateId_Key
    ON Setting_EmailTemplateParameters (EmailTemplateId, ParamKey)
    WHERE IsDeleted = 0;
