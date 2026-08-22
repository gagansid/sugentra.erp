-- Seed: sample Roles + Permissions catalog for Phase 1 login/authorization testing.
-- SuperAdmin role gets every permission granted; other roles are left unassigned for now (assign via Identity_UserPermissions/RolePermissions later).

INSERT INTO Identity_Roles (Name, Description, CreatedBy)
SELECT v.Name, v.Description, NULL
FROM (VALUES
    ('Admin',        'Administrative access to manage users, roles, and settings.'),
    ('Manager',      'Manages day-to-day operations, can approve transactions.'),
    ('Staff',        'Standard operational user with limited access.'),
    ('Viewer',       'Read-only access across modules.')
) AS v(Name, Description)
WHERE NOT EXISTS (SELECT 1 FROM Identity_Roles WHERE Name = v.Name);

INSERT INTO Identity_Permissions (Code, Module, Description)
SELECT v.Code, v.Module, v.Description
FROM (VALUES
    ('User_View',        'Identity', 'View users'),
    ('User_Create',      'Identity', 'Create users'),
    ('User_Update',      'Identity', 'Update users'),
    ('User_Delete',      'Identity', 'Deactivate/delete users'),
    ('Role_View',         'Identity', 'View roles'),
    ('Role_Create',       'Identity', 'Create roles'),
    ('Role_Update',       'Identity', 'Update roles'),
    ('Role_Delete',       'Identity', 'Delete roles'),
    ('Permission_View',   'Identity', 'View permissions'),
    ('Permission_Assign', 'Identity', 'Assign permissions to roles/users'),
    ('Settings_View',     'Settings', 'View settings/reference data'),
    ('Settings_Manage',   'Settings', 'Manage settings/reference data'),
    ('AuditLog_View',     'AuditLog', 'View audit logs')
) AS v(Code, Module, Description)
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = v.Code);

-- Grant SuperAdmin every permission currently in the catalog.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name = 'SuperAdmin'
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );
