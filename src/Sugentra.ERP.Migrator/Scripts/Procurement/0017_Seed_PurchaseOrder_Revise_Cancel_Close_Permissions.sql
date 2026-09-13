DECLARE @Perms TABLE (Code NVARCHAR(100), Description NVARCHAR(300));
INSERT INTO @Perms (Code, Description) VALUES
    ('PurchaseOrder_Revise', 'Create a revision (amendment) of an approved purchase order'),
    ('PurchaseOrder_Cancel', 'Cancel a purchase order'),
    ('PurchaseOrder_Close',  'Close the un-fulfilled remainder of a purchase order');

INSERT INTO Identity_Permissions (Code, Module, ModuleId, Description)
SELECT p.Code, 'Procurement', m.Id, p.Description
FROM @Perms p
JOIN Setting_Modules m ON m.Code = 'Procurement'
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = p.Code);

INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
JOIN @Perms np ON np.Code = p.Code
WHERE r.Name IN ('SuperAdmin', 'Admin', 'Super Admin')
  AND NOT EXISTS (SELECT 1 FROM Identity_RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id);
GO
