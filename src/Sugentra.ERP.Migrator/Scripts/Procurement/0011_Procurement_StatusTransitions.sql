-- Procurement_StatusTransitions: config/lookup table describing which LifecycleStatus transitions are
-- allowed per document type (CategoryGroup), so the allowed-transition graph lives in data instead of being
-- hardcoded across UseCases. No code reads/writes this table yet - part of the not-yet-implemented
-- "Post-Approval Correction Workflow" plan (see docs/modules/procurement.md). LifecycleStatus itself
-- (the new column on Procurement_PurchaseOrders/Procurement_PurchaseRequisitions) does not exist yet either;
-- this table is seeded ahead of time so the transition rules are captured now.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Procurement_StatusTransitions')
BEGIN
    CREATE TABLE Procurement_StatusTransitions
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Procurement_StatusTransitions PRIMARY KEY,
        CategoryGroup        NVARCHAR(30)   NOT NULL, -- 'PurchaseOrder' | 'PurchaseRequisition'
        FromStatus           NVARCHAR(30)   NOT NULL,
        ToStatus             NVARCHAR(30)   NOT NULL,
        ConditionDescription NVARCHAR(200)  NOT NULL,
        IsSystemTriggered    BIT            NOT NULL CONSTRAINT DF_Procurement_StatusTransitions_IsSystemTriggered DEFAULT (0), -- true when the system sets it automatically (e.g. Superseded), false when a user action triggers it
        CreatedAt            DATETIME2      NOT NULL CONSTRAINT DF_Procurement_StatusTransitions_CreatedAt DEFAULT (GETDATE()),

        CONSTRAINT UQ_Procurement_StatusTransitions UNIQUE (CategoryGroup, FromStatus, ToStatus),
        CONSTRAINT CK_Procurement_StatusTransitions_CategoryGroup CHECK (CategoryGroup IN ('PurchaseOrder', 'PurchaseRequisition'))
    );

    INSERT INTO Procurement_StatusTransitions (CategoryGroup, FromStatus, ToStatus, ConditionDescription, IsSystemTriggered)
    VALUES
        ('PurchaseOrder', 'Draft',             'Approved',          'Full approval hierarchy passed', 0),
        ('PurchaseOrder', 'Draft',             'Cancelled',         'Cancelled by creator/Procurement Manager before approval', 0),
        ('PurchaseOrder', 'PendingApproval',   'Approved',          'Full approval hierarchy passed', 0),
        ('PurchaseOrder', 'PendingApproval',   'Cancelled',         'Cancelled by creator/Procurement Manager before approval', 0),
        ('PurchaseOrder', 'Approved',          'PartiallyReceived', 'Warehouse posts a Goods Receipt for part of the quantity', 1),
        ('PurchaseOrder', 'Approved',          'FullyReceived',     'Warehouse posts a Goods Receipt for 100% of the quantity', 1),
        ('PurchaseOrder', 'Approved',          'Cancelled',         'No Goods Receipt and no Invoice exist yet', 0),
        ('PurchaseOrder', 'Approved',          'Superseded',        'System-set automatically when "Revise PO" is confirmed and the new PO is created', 1),
        ('PurchaseOrder', 'PartiallyReceived', 'FullyReceived',     'Warehouse receives the remaining quantity', 1),
        ('PurchaseOrder', 'PartiallyReceived', 'Closed',            'Remaining quantity force-closed by Admin/Manager (vendor won''t ship the rest)', 0),
        ('PurchaseOrder', 'PartiallyReceived', 'Superseded',        'System-set automatically when "Revise PO" is confirmed and the new PO is created', 1),
        ('PurchaseRequisition', 'Approved',    'Closed',            'Remaining un-ordered line quantity force-closed by Admin/Manager', 0);
END
GO
