-- Split the coarse MasterData_Manage permission into MasterData_Create/MasterData_Edit/MasterData_Delete
-- (View stays as-is). Any role/user previously granted Manage gets all three new permissions, then the
-- old Manage permission (and its grant rows) is hard-deleted, same pattern as 0037's Settings_View/Manage cleanup.
INSERT INTO Identity_Permissions (Code, Module, Description)
SELECT v.Code, v.Module, v.Description
FROM (VALUES
    ('MasterData_Create', 'MasterData', 'Create business partners, items, BOM, price lists'),
    ('MasterData_Edit',   'MasterData', 'Edit business partners, items, BOM, price lists'),
    ('MasterData_Delete', 'MasterData', 'Delete business partners, items, BOM, price lists')
) AS v(Code, Module, Description)
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = v.Code);

-- Role grants: every role that had MasterData_Manage gets all three new permissions.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT rp.RoleId, np.Id, NULL
FROM Identity_RolePermissions rp
JOIN Identity_Permissions oldp ON oldp.Id = rp.PermissionId AND oldp.Code = 'MasterData_Manage'
CROSS JOIN Identity_Permissions np
WHERE np.Code IN ('MasterData_Create', 'MasterData_Edit', 'MasterData_Delete')
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions x
      WHERE x.RoleId = rp.RoleId AND x.PermissionId = np.Id
  );

-- User-level overrides (allow/deny): mirror any existing MasterData_Manage override onto all three new permissions.
INSERT INTO Identity_UserPermissions (UserId, PermissionId, IsAllowed, CreatedBy)
SELECT up.UserId, np.Id, up.IsAllowed, NULL
FROM Identity_UserPermissions up
JOIN Identity_Permissions oldp ON oldp.Id = up.PermissionId AND oldp.Code = 'MasterData_Manage'
CROSS JOIN Identity_Permissions np
WHERE np.Code IN ('MasterData_Create', 'MasterData_Edit', 'MasterData_Delete')
  AND NOT EXISTS (
      SELECT 1 FROM Identity_UserPermissions x
      WHERE x.UserId = up.UserId AND x.PermissionId = np.Id
  );

-- Hard-delete the superseded MasterData_Manage permission and its grant/override rows.
DELETE FROM Identity_UserPermissions WHERE PermissionId IN (SELECT Id FROM Identity_Permissions WHERE Code = 'MasterData_Manage');
DELETE FROM Identity_RolePermissions WHERE PermissionId IN (SELECT Id FROM Identity_Permissions WHERE Code = 'MasterData_Manage');
DELETE FROM Identity_Permissions WHERE Code = 'MasterData_Manage';
