-- Cross-module upload metadata table (no owning module - used by any feature that needs file uploads).
CREATE TABLE Shared_Uploads (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Category NVARCHAR(50) NOT NULL,
    EntityType NVARCHAR(100) NULL,
    EntityId BIGINT NULL,
    FileName NVARCHAR(255) NOT NULL,
    StoredFileName NVARCHAR(255) NOT NULL,
    Extension NVARCHAR(20) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    SizeBytes BIGINT NOT NULL,
    Url NVARCHAR(500) NOT NULL,
    StoragePath NVARCHAR(500) NOT NULL,
    StorageProvider NVARCHAR(20) NOT NULL DEFAULT 'Local',
    Width INT NULL,
    Height INT NULL,
    DurationSeconds INT NULL,
    IsPublic BIT NOT NULL DEFAULT 0,
    Checksum NVARCHAR(64) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy BIGINT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy BIGINT NULL
);
GO

CREATE INDEX IX_Shared_Uploads_Category ON Shared_Uploads (Category);
CREATE INDEX IX_Shared_Uploads_Entity ON Shared_Uploads (EntityType, EntityId);
GO
