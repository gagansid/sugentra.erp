-- Procurement_PurchaseOrderLineSources: per-PO-line breakdown of which Purchase Requisition (and PR line)
-- contributed how much quantity, for audit/traceability after multiple PRs are merged into one PO line.
-- Rows with NULL PurchaseRequisitionId represent quantity added manually (not sourced from any PR).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Procurement_PurchaseOrderLineSources')
BEGIN
    CREATE TABLE Procurement_PurchaseOrderLineSources
    (
        Id                          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Procurement_PurchaseOrderLineSources PRIMARY KEY,
        PurchaseOrderLineId         BIGINT         NOT NULL,
        PurchaseRequisitionId       BIGINT         NULL,
        PurchaseRequisitionLineId   BIGINT         NULL,
        Quantity                    DECIMAL(18,2)  NOT NULL,

        CreatedAt                   DATETIME2      NOT NULL CONSTRAINT DF_Procurement_PurchaseOrderLineSources_CreatedAt DEFAULT (GETDATE()),
        CreatedBy                   BIGINT         NULL,
        UpdatedAt                   DATETIME2      NULL,
        UpdatedBy                   BIGINT         NULL,
        IsDeleted                   BIT            NOT NULL CONSTRAINT DF_Procurement_PurchaseOrderLineSources_IsDeleted DEFAULT (0),
        DeletedAt                   DATETIME2      NULL,
        DeletedBy                   BIGINT         NULL,

        CONSTRAINT FK_Procurement_PurchaseOrderLineSources_Line FOREIGN KEY (PurchaseOrderLineId) REFERENCES Procurement_PurchaseOrderLines (Id),
        CONSTRAINT FK_Procurement_PurchaseOrderLineSources_Requisition FOREIGN KEY (PurchaseRequisitionId) REFERENCES Procurement_PurchaseRequisitions (Id),
        CONSTRAINT FK_Procurement_PurchaseOrderLineSources_RequisitionLine FOREIGN KEY (PurchaseRequisitionLineId) REFERENCES Procurement_PurchaseRequisitionLines (Id)
    );

    CREATE INDEX IX_Procurement_PurchaseOrderLineSources_Line ON Procurement_PurchaseOrderLineSources (PurchaseOrderLineId);
END
GO
