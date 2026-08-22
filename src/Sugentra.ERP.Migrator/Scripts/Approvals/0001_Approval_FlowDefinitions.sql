-- Approval_FlowDefinitions: reusable, dynamic approval flow configs, selected per DocumentType + optional
-- amount/currency/warehouse condition (see docs/plan.md ApprovalMatrix precedent). Higher Priority wins ties.
CREATE TABLE Approval_FlowDefinitions
(
    Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Approval_FlowDefinitions PRIMARY KEY,
    DocumentType NVARCHAR(50)   NOT NULL,
    Name         NVARCHAR(150)  NOT NULL,
    MinAmount    DECIMAL(18,2)  NULL,
    MaxAmount    DECIMAL(18,2)  NULL,
    CurrencyId   BIGINT         NULL,
    WarehouseId  BIGINT         NULL,
    Priority     INT            NOT NULL CONSTRAINT DF_Approval_FlowDefinitions_Priority DEFAULT (0),
    IsActive     BIT            NOT NULL CONSTRAINT DF_Approval_FlowDefinitions_IsActive DEFAULT (1),

    CreatedAt    DATETIME2      NOT NULL CONSTRAINT DF_Approval_FlowDefinitions_CreatedAt DEFAULT (GETDATE()),
    CreatedBy    BIGINT         NULL,
    UpdatedAt    DATETIME2      NULL,
    UpdatedBy    BIGINT         NULL,
    IsDeleted    BIT            NOT NULL CONSTRAINT DF_Approval_FlowDefinitions_IsDeleted DEFAULT (0),
    DeletedAt    DATETIME2      NULL,
    DeletedBy    BIGINT         NULL,

    CONSTRAINT FK_Approval_FlowDefinitions_Currency FOREIGN KEY (CurrencyId) REFERENCES Setting_Currencies (Id),
    CONSTRAINT FK_Approval_FlowDefinitions_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Setting_Warehouses (Id)
);

CREATE INDEX IX_Approval_FlowDefinitions_DocumentType ON Approval_FlowDefinitions (DocumentType);
GO
