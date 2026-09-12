-- Procurement_PurchaseOrderLines: item/qty/price lines for a Purchase Order header.
-- ReceivedQuantity is updated by Procurement when Inventory posts a Goods Receipt referencing this PO
-- (see IPurchaseOrderReceiptService contract) - Procurement never reads Inventory's tables directly.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Procurement_PurchaseOrderLines')
BEGIN
    CREATE TABLE Procurement_PurchaseOrderLines
    (
        Id                BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Procurement_PurchaseOrderLines PRIMARY KEY,
        PurchaseOrderId   BIGINT         NOT NULL,
        ItemId            BIGINT         NOT NULL,
        WarehouseId       BIGINT         NOT NULL,
        Quantity          DECIMAL(18,2)  NOT NULL,
        UnitPrice         DECIMAL(18,2)  NOT NULL,
        DiscountPercent   DECIMAL(5,2)   NOT NULL CONSTRAINT DF_Procurement_PurchaseOrderLines_DiscountPercent DEFAULT (0),
        ReceivedQuantity  DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Procurement_PurchaseOrderLines_ReceivedQuantity DEFAULT (0),

        CreatedAt         DATETIME2      NOT NULL CONSTRAINT DF_Procurement_PurchaseOrderLines_CreatedAt DEFAULT (GETDATE()),
        CreatedBy         BIGINT         NULL,
        UpdatedAt         DATETIME2      NULL,
        UpdatedBy         BIGINT         NULL,
        IsDeleted         BIT            NOT NULL CONSTRAINT DF_Procurement_PurchaseOrderLines_IsDeleted DEFAULT (0),
        DeletedAt         DATETIME2      NULL,
        DeletedBy         BIGINT         NULL,

        CONSTRAINT FK_Procurement_PurchaseOrderLines_Order FOREIGN KEY (PurchaseOrderId) REFERENCES Procurement_PurchaseOrders (Id),
        CONSTRAINT FK_Procurement_PurchaseOrderLines_Item FOREIGN KEY (ItemId) REFERENCES MasterData_Items (Id),
        CONSTRAINT FK_Procurement_PurchaseOrderLines_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Setting_Warehouses (Id)
    );

    CREATE INDEX IX_Procurement_PurchaseOrderLines_Order ON Procurement_PurchaseOrderLines (PurchaseOrderId);
END
GO
