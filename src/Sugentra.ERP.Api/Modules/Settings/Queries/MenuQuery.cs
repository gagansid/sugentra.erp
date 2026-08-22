using Sugentra.ERP.Api.Modules.Settings.Dtos;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Settings.Queries;

/// <summary>Read path: raw Dapper, no business rules — powers the admin menu list/tree page.</summary>
public class MenuQuery(IDbConnectionFactory connectionFactory)
{
    private const string Sql = """
        SELECT me.Id, me.ModuleId, mo.Code AS ModuleCode, mo.Name AS ModuleName,
               me.ParentId, parent.Name AS ParentName,
               me.Name, me.Icon, me.Controller, me.Action, me.RequiredPermission,
               me.SortOrder, me.IsActive
        FROM Setting_Menus me
        JOIN Setting_Modules mo ON mo.Id = me.ModuleId
        LEFT JOIN Setting_Menus parent ON parent.Id = me.ParentId
        WHERE me.IsDeleted = 0
        ORDER BY mo.SortOrder, ISNULL(me.ParentId, me.Id), me.SortOrder
        """;

    public async Task<IReadOnlyList<MenuListItem>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryListAsync<MenuListItem>(Sql);
    }

    private const string ActiveSql = """
        SELECT me.Id, me.ParentId, me.Name, me.Icon, me.Controller, me.Action, me.RequiredPermission, me.SortOrder,
               mo.Id AS ModuleId, mo.Code AS ModuleCode, mo.Name AS ModuleName, mo.Icon AS ModuleIcon,
               mo.SortOrder AS ModuleSortOrder, CASE WHEN mo.Route IS NULL THEN 0 ELSE 1 END AS HasWorkspace
        FROM Setting_Menus me
        JOIN Setting_Modules mo ON mo.Id = me.ModuleId AND mo.IsDeleted = 0
        WHERE me.IsDeleted = 0 AND me.IsActive = 1
        ORDER BY mo.SortOrder, ISNULL(me.ParentId, me.Id), me.SortOrder
        """;

    /// <summary>Powers the fully dynamic sidebar: builds a module -> menu tree, keeping only items the caller's
    /// permission claims allow. If a node's parent got filtered out, the node is promoted to top-level instead
    /// of being silently dropped, so a permitted sub-menu is never hidden by a denied parent.</summary>
    public async Task<IReadOnlyList<MenuModuleGroup>> GetActiveForPermissionCodesAsync(IEnumerable<string> permissionCodes)
    {
        var codes = permissionCodes.ToHashSet();
        using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryListAsync<MenuRow>(ActiveSql);

        var visible = rows.Where(r => r.RequiredPermission is null || codes.Contains(r.RequiredPermission)).ToList();
        var visibleIds = visible.Select(r => r.Id).ToHashSet();
        var nodesById = visible.ToDictionary(r => r.Id, r => new MenuNode
        {
            Id = r.Id,
            Name = r.Name,
            Icon = r.Icon,
            Controller = r.Controller,
            Action = r.Action
        });

        var groups = new List<MenuModuleGroup>();
        foreach (var moduleRows in visible
                     .GroupBy(r => (r.ModuleId, r.ModuleCode, r.ModuleName, r.ModuleIcon, r.HasWorkspace, r.ModuleSortOrder))
                     .OrderBy(g => g.Key.ModuleSortOrder))
        {
            var group = new MenuModuleGroup
            {
                ModuleCode = moduleRows.Key.ModuleCode,
                ModuleName = moduleRows.Key.ModuleName,
                ModuleIcon = moduleRows.Key.ModuleIcon,
                HasWorkspace = moduleRows.Key.HasWorkspace
            };

            foreach (var row in moduleRows.OrderBy(r => r.SortOrder))
            {
                var node = nodesById[row.Id];
                if (row.ParentId is null || !visibleIds.Contains(row.ParentId.Value))
                {
                    group.Items.Add(node);
                }
                else
                {
                    nodesById[row.ParentId.Value].Children.Add(node);
                }
            }

            if (group.Items.Count > 0)
            {
                groups.Add(group);
            }
        }

        return groups;
    }

    private class MenuRow
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? RequiredPermission { get; set; }
        public int SortOrder { get; set; }
        public long ModuleId { get; set; }
        public string ModuleCode { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string? ModuleIcon { get; set; }
        public int ModuleSortOrder { get; set; }
        public bool HasWorkspace { get; set; }
    }
}
