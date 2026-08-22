-- Setting_ApprovalMatrices: approval-workflow metadata (Phase 1 = storage only, no execution engine yet).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_ApprovalMatrices')
BEGIN
    CREATE TABLE Setting_ApprovalMatrices
    (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_ApprovalMatrices PRIMARY KEY,
        DocumentType    NVARCHAR(50)   NOT NULL,
        MinAmount       DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Setting_ApprovalMatrices_MinAmount DEFAULT (0),
        MaxAmount       DECIMAL(18,2)  NULL,
        CurrencyId      BIGINT         NULL,
        ApprovalLevel   INT            NOT NULL,
        ApproverRoleId  BIGINT         NOT NULL,

        CreatedAt       DATETIME2      NOT NULL CONSTRAINT DF_Setting_ApprovalMatrices_CreatedAt DEFAULT (GETDATE()),
        CreatedBy       BIGINT         NULL,
        UpdatedAt       DATETIME2      NULL,
        UpdatedBy       BIGINT         NULL,
        IsDeleted       BIT            NOT NULL CONSTRAINT DF_Setting_ApprovalMatrices_IsDeleted DEFAULT (0),
        DeletedAt       DATETIME2      NULL,
        DeletedBy       BIGINT         NULL,

        CONSTRAINT FK_Setting_ApprovalMatrices_Currency FOREIGN KEY (CurrencyId) REFERENCES Setting_Currencies (Id),
        CONSTRAINT FK_Setting_ApprovalMatrices_ApproverRole FOREIGN KEY (ApproverRoleId) REFERENCES Identity_Roles (Id)
    );
END
