-- UI Kit's "Tables" menu item has no RequiredPermission at all, so it can never satisfy the EXISTS match in
-- ModuleQuery.GetActiveForPermissionCodesAsync either - add a dedicated view permission just so the UIKit tile can show up on the hub.
INSERT INTO Identity_Permissions (Code, Module, Description)
SELECT 'UIKit_View', 'UIKit', 'View UI Kit demo pages'
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = 'UIKit_View');

INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE p.Code = 'UIKit_View'
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );
GO
