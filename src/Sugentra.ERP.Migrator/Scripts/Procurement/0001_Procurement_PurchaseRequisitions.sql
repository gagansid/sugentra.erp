-- Procurement_PurchaseRequisitions: internal request for goods/services, optionally preceding a Purchase Order.
-- Approval workflow via Approval_FlowDefinitions (DocumentType = 'PurchaseRequisition'), same engine as Inventory's StockMutation.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Procurement_PurchaseRequisitions')
BEGIN
    CREATE TABLE Procurement_PurchaseRequisitions
    (
        Id               BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Procurement_PurchaseRequisitions PRIMARY KEY,
        -- Generated via IDocumentNumberGeneratorService.GetNextAsync("PurchaseRequisition"), never hand-entered.
        RequisitionNumber NVARCHAR(50)  NOT NULL,
        RequesterUserId  BIGINT         NOT NULL,
        WarehouseId      BIGINT         NOT NULL,
        RequisitionDate  DATETIME2      NOT NULL,
        Status           NVARCHAR(20)   NOT NULL CONSTRAINT DF_Procurement_PurchaseRequisitions_Status DEFAULT ('Draft'),
        -- Set alongside Status == 'WaitingApproval'; human-readable label composed in the UI, not stored.
        CurrentApprovalLevel NVARCHAR(100) NULL,
        Notes            NVARCHAR(500)  NULL,

        CreatedAt        DATETIME2      NOT NULL CONSTRAINT DF_Procurement_PurchaseRequisitions_CreatedAt DEFAULT (GETDATE()),
        CreatedBy        BIGINT         NULL,
        UpdatedAt        DATETIME2      NULL,
        UpdatedBy        BIGINT         NULL,
        IsDeleted        BIT            NOT NULL CONSTRAINT DF_Procurement_PurchaseRequisitions_IsDeleted DEFAULT (0),
        DeletedAt        DATETIME2      NULL,
        DeletedBy        BIGINT         NULL,

        CONSTRAINT UQ_Procurement_PurchaseRequisitions_Number UNIQUE (RequisitionNumber),
        CONSTRAINT CK_Procurement_PurchaseRequisitions_Status CHECK (Status IN ('Draft', 'WaitingApproval', 'Approved', 'Rejected', 'Closed')),
        CONSTRAINT FK_Procurement_PurchaseRequisitions_Requester FOREIGN KEY (RequesterUserId) REFERENCES Identity_Users (Id),
        CONSTRAINT FK_Procurement_PurchaseRequisitions_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Setting_Warehouses (Id)
    );
END
GO
