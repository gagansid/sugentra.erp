-- Procurement_PurchaseOrders: formal order to a vendor, optionally sourced from an Approved Purchase Requisition.
-- Approval workflow via Approval_FlowDefinitions (DocumentType = 'PurchaseOrder'), same engine as Inventory's StockMutation.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Procurement_PurchaseOrders')
BEGIN
    CREATE TABLE Procurement_PurchaseOrders
    (
        Id               BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Procurement_PurchaseOrders PRIMARY KEY,
        -- Generated via IDocumentNumberGeneratorService.GetNextAsync("PurchaseOrder"), never hand-entered.
        OrderNumber      NVARCHAR(50)   NOT NULL,
        -- Nullable: a PO can be raised directly, without going through a Purchase Requisition first.
        PurchaseRequisitionId BIGINT    NULL,
        VendorId         BIGINT         NOT NULL,
        CurrencyId       BIGINT         NOT NULL,
        PaymentTermDays  INT            NULL,
        OrderDate        DATETIME2      NOT NULL,
        ExpectedDeliveryDate DATETIME2  NULL,
        Status           NVARCHAR(20)   NOT NULL CONSTRAINT DF_Procurement_PurchaseOrders_Status DEFAULT ('Draft'),
        -- Set alongside Status == 'WaitingApproval'; human-readable label composed in the UI, not stored.
        CurrentApprovalLevel NVARCHAR(100) NULL,
        Notes            NVARCHAR(500)  NULL,

        CreatedAt        DATETIME2      NOT NULL CONSTRAINT DF_Procurement_PurchaseOrders_CreatedAt DEFAULT (GETDATE()),
        CreatedBy        BIGINT         NULL,
        UpdatedAt        DATETIME2      NULL,
        UpdatedBy        BIGINT         NULL,
        IsDeleted        BIT            NOT NULL CONSTRAINT DF_Procurement_PurchaseOrders_IsDeleted DEFAULT (0),
        DeletedAt        DATETIME2      NULL,
        DeletedBy        BIGINT         NULL,

        CONSTRAINT UQ_Procurement_PurchaseOrders_Number UNIQUE (OrderNumber),
        CONSTRAINT CK_Procurement_PurchaseOrders_Status CHECK (Status IN ('Draft', 'WaitingApproval', 'Approved', 'Rejected', 'PartiallyReceived', 'FullyReceived', 'Closed')),
        CONSTRAINT FK_Procurement_PurchaseOrders_Requisition FOREIGN KEY (PurchaseRequisitionId) REFERENCES Procurement_PurchaseRequisitions (Id),
        CONSTRAINT FK_Procurement_PurchaseOrders_Vendor FOREIGN KEY (VendorId) REFERENCES MasterData_BusinessPartners (Id),
        CONSTRAINT FK_Procurement_PurchaseOrders_Currency FOREIGN KEY (CurrencyId) REFERENCES Setting_Currencies (Id)
    );
END
GO
