-- Procurement_PurchaseRequisitionLines: item/qty lines for a Purchase Requisition header.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Procurement_PurchaseRequisitionLines')
BEGIN
    CREATE TABLE Procurement_PurchaseRequisitionLines
    (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Procurement_PurchaseRequisitionLines PRIMARY KEY,
        RequisitionId   BIGINT         NOT NULL,
        ItemId          BIGINT         NOT NULL,
        Quantity        DECIMAL(18,2)  NOT NULL,
        Notes           NVARCHAR(200)  NULL,

        CreatedAt       DATETIME2      NOT NULL CONSTRAINT DF_Procurement_PurchaseRequisitionLines_CreatedAt DEFAULT (GETDATE()),
        CreatedBy       BIGINT         NULL,
        UpdatedAt       DATETIME2      NULL,
        UpdatedBy       BIGINT         NULL,
        IsDeleted       BIT            NOT NULL CONSTRAINT DF_Procurement_PurchaseRequisitionLines_IsDeleted DEFAULT (0),
        DeletedAt       DATETIME2      NULL,
        DeletedBy       BIGINT         NULL,

        CONSTRAINT FK_Procurement_PurchaseRequisitionLines_Requisition FOREIGN KEY (RequisitionId) REFERENCES Procurement_PurchaseRequisitions (Id),
        CONSTRAINT FK_Procurement_PurchaseRequisitionLines_Item FOREIGN KEY (ItemId) REFERENCES MasterData_Items (Id)
    );

    CREATE INDEX IX_Procurement_PurchaseRequisitionLines_Requisition ON Procurement_PurchaseRequisitionLines (RequisitionId);
END
GO
