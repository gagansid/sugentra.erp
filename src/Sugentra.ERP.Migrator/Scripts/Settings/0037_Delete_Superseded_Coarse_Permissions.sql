-- Old coarse permission codes fully superseded by 0036's per-entity Create/Edit/Delete/Report/View codes.
-- No controller/view references them anymore (confirmed via repo-wide grep) - safe to remove outright,
-- including their role/user grants, rather than leaving them as orphaned rows.
DECLARE @Codes TABLE (Code NVARCHAR(100));
INSERT INTO @Codes (Code) VALUES ('Settings_View'), ('Settings_Manage'), ('Module_Manage'), ('Menu_Manage');

DELETE rp FROM Identity_RolePermissions rp
JOIN Identity_Permissions p ON p.Id = rp.PermissionId
JOIN @Codes c ON c.Code = p.Code;

DELETE up FROM Identity_UserPermissions up
JOIN Identity_Permissions p ON p.Id = up.PermissionId
JOIN @Codes c ON c.Code = p.Code;

DELETE p FROM Identity_Permissions p
JOIN @Codes c ON c.Code = p.Code;
GO
