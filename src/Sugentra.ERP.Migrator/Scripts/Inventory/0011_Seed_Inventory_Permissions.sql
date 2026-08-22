-- Inventory permissions: Batch/QuarantineHold/StockMutation/StockOpname (full CRUD) + StockBalance/StockLedger (view-only reports).
DECLARE @Perms TABLE (Code NVARCHAR(100), Description NVARCHAR(300));
INSERT INTO @Perms (Code, Description) VALUES
    ('Batch_Create', 'Create inventory batches'),
    ('Batch_Edit',   'Edit inventory batches'),
    ('Batch_Delete', 'Delete inventory batches'),
    ('Batch_Report', 'Run batch reports'),
    ('Batch_View',   'View inventory batches'),

    ('QuarantineHold_Create', 'Create quarantine holds'),
    ('QuarantineHold_Edit',   'Edit quarantine holds'),
    ('QuarantineHold_Delete', 'Delete quarantine holds'),
    ('QuarantineHold_Report', 'Run quarantine hold reports'),
    ('QuarantineHold_View',   'View quarantine holds'),

    ('StockMutation_Create', 'Create stock mutations'),
    ('StockMutation_Edit',   'Edit/approve/complete stock mutations'),
    ('StockMutation_Delete', 'Delete stock mutations'),
    ('StockMutation_Report', 'Run stock mutation reports'),
    ('StockMutation_View',   'View stock mutations'),

    ('StockOpname_Create', 'Create stock opnames'),
    ('StockOpname_Edit',   'Edit/submit/approve stock opnames'),
    ('StockOpname_Delete', 'Delete stock opnames'),
    ('StockOpname_Report', 'Run stock opname reports'),
    ('StockOpname_View',   'View stock opnames'),

    ('StockBalance_Report', 'Run stock balance reports'),
    ('StockBalance_View',   'View stock balances'),

    ('StockLedger_Report', 'Run stock ledger (kartu stok) reports'),
    ('StockLedger_View',   'View stock ledger (kartu stok)');

INSERT INTO Identity_Permissions (Code, Module, ModuleId, Description)
SELECT p.Code, 'Inventory', m.Id, p.Description
FROM @Perms p
JOIN Setting_Modules m ON m.Code = 'Inventory'
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = p.Code);

-- SuperAdmin + Admin: full access.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
JOIN @Perms np ON np.Code = p.Code
WHERE r.Name IN ('SuperAdmin', 'Admin')
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );

-- Manager/Staff/Viewer: view-only.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
JOIN @Perms np ON np.Code = p.Code
WHERE r.Name IN ('Manager', 'Staff', 'Viewer')
  AND p.Code LIKE '%\_View' ESCAPE '\'
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );
GO
