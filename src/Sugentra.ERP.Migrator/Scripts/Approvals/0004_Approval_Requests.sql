-- Approval_Requests: one instance of a document going through approval. DocumentType/DocumentId are a
-- polymorphic pointer (string discriminator, no FK) to the owning module's row — same pattern as
-- Inventory_StockLedgers.ReferenceType/ReferenceId, since the document may live in another module.
CREATE TABLE Approval_Requests
(
    Id                 BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Approval_Requests PRIMARY KEY,
    DocumentType       NVARCHAR(50)   NOT NULL,
    DocumentId         BIGINT         NOT NULL,
    DocumentNumber     NVARCHAR(100)  NOT NULL,
    FlowDefinitionId   BIGINT         NOT NULL,
    Amount             DECIMAL(18,2)  NULL,
    CurrencyId         BIGINT         NULL,
    WarehouseId        BIGINT         NULL,
    Status             NVARCHAR(20)   NOT NULL CONSTRAINT DF_Approval_Requests_Status DEFAULT ('Pending'),
    CurrentLevelNumber INT            NOT NULL CONSTRAINT DF_Approval_Requests_CurrentLevelNumber DEFAULT (1),
    RequestedBy        BIGINT         NOT NULL,
    RequestedAt        DATETIME2      NOT NULL CONSTRAINT DF_Approval_Requests_RequestedAt DEFAULT (GETDATE()),
    CompletedAt        DATETIME2      NULL,

    CreatedAt          DATETIME2      NOT NULL CONSTRAINT DF_Approval_Requests_CreatedAt DEFAULT (GETDATE()),
    CreatedBy          BIGINT         NULL,
    UpdatedAt          DATETIME2      NULL,
    UpdatedBy          BIGINT         NULL,
    IsDeleted          BIT            NOT NULL CONSTRAINT DF_Approval_Requests_IsDeleted DEFAULT (0),
    DeletedAt          DATETIME2      NULL,
    DeletedBy          BIGINT         NULL,

    CONSTRAINT FK_Approval_Requests_FlowDefinition FOREIGN KEY (FlowDefinitionId) REFERENCES Approval_FlowDefinitions (Id)
);

CREATE INDEX IX_Approval_Requests_Document ON Approval_Requests (DocumentType, DocumentId);
GO
