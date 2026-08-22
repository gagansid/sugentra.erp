-- Permission codes for the Approvals module — flow configuration + inbox/act.
INSERT INTO Identity_Permissions (Code, Module, Description)
SELECT v.Code, v.Module, v.Description
FROM (VALUES
    ('ApprovalFlow_View',    'Approvals', 'View approval flow definitions'),
    ('ApprovalFlow_Create',  'Approvals', 'Create approval flow definitions'),
    ('ApprovalFlow_Edit',    'Approvals', 'Edit approval flow definitions'),
    ('ApprovalFlow_Delete',  'Approvals', 'Delete approval flow definitions'),
    ('ApprovalRequest_View', 'Approvals', 'View approval requests / inbox'),
    ('ApprovalRequest_Act',  'Approvals', 'Approve or reject approval requests')
) AS v(Code, Module, Description)
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = v.Code);
GO

INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name IN ('SuperAdmin', 'Admin')
  AND p.Code IN ('ApprovalFlow_View', 'ApprovalFlow_Create', 'ApprovalFlow_Edit', 'ApprovalFlow_Delete',
                 'ApprovalRequest_View', 'ApprovalRequest_Act')
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );
GO
