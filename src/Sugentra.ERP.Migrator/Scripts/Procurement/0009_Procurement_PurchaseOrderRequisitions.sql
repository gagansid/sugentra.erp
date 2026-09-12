-- Replaces the single nullable PurchaseRequisitionId column on Procurement_PurchaseOrders with a
-- many-to-many junction table: one PO can now be sourced from multiple PRs (and, per current business
-- rules, a PR may also be split across multiple POs).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Procurement_PurchaseOrderRequisitions')
BEGIN
    CREATE TABLE Procurement_PurchaseOrderRequisitions
    (
        Id                     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Procurement_PurchaseOrderRequisitions PRIMARY KEY,
        PurchaseOrderId        BIGINT NOT NULL,
        PurchaseRequisitionId  BIGINT NOT NULL,

        CreatedAt              DATETIME2 NOT NULL CONSTRAINT DF_Procurement_PurchaseOrderRequisitions_CreatedAt DEFAULT (GETDATE()),
        CreatedBy              BIGINT NULL,
        UpdatedAt              DATETIME2 NULL,
        UpdatedBy              BIGINT NULL,
        IsDeleted              BIT NOT NULL CONSTRAINT DF_Procurement_PurchaseOrderRequisitions_IsDeleted DEFAULT (0),
        DeletedAt              DATETIME2 NULL,
        DeletedBy              BIGINT NULL,

        CONSTRAINT FK_Procurement_PurchaseOrderRequisitions_Order FOREIGN KEY (PurchaseOrderId) REFERENCES Procurement_PurchaseOrders (Id),
        CONSTRAINT FK_Procurement_PurchaseOrderRequisitions_Requisition FOREIGN KEY (PurchaseRequisitionId) REFERENCES Procurement_PurchaseRequisitions (Id)
    );

    CREATE INDEX IX_Procurement_PurchaseOrderRequisitions_Order ON Procurement_PurchaseOrderRequisitions (PurchaseOrderId);
    CREATE INDEX IX_Procurement_PurchaseOrderRequisitions_Requisition ON Procurement_PurchaseOrderRequisitions (PurchaseRequisitionId);
END
GO

INSERT INTO Procurement_PurchaseOrderRequisitions (PurchaseOrderId, PurchaseRequisitionId, CreatedBy)
SELECT Id, PurchaseRequisitionId, CreatedBy
FROM Procurement_PurchaseOrders
WHERE PurchaseRequisitionId IS NOT NULL;
GO

ALTER TABLE Procurement_PurchaseOrders DROP CONSTRAINT FK_Procurement_PurchaseOrders_Requisition;
ALTER TABLE Procurement_PurchaseOrders DROP COLUMN PurchaseRequisitionId;
GO
