-- Permission codes for the Approval Role Category admin screen (mapping only — no Edit, Create/Delete suffice).
INSERT INTO Identity_Permissions (Code, Module, Description)
SELECT v.Code, v.Module, v.Description
FROM (VALUES
    ('ApprovalRoleCategory_View',   'Approvals', 'View approval role categories'),
    ('ApprovalRoleCategory_Create', 'Approvals', 'Create approval role categories'),
    ('ApprovalRoleCategory_Delete', 'Approvals', 'Delete approval role categories')
) AS v(Code, Module, Description)
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = v.Code);
GO

INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name IN ('SuperAdmin', 'Admin')
  AND p.Code IN ('ApprovalRoleCategory_View', 'ApprovalRoleCategory_Create', 'ApprovalRoleCategory_Delete')
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );
GO
