using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Services.Inventory;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers;

// Landing page for a dashboard module tile (Setting_Modules.Route points here) - shows that module's own
// stats/charts (Inventory only, for now) before the user drills into any specific sub-page.
public class WorkspaceController(MenuApiService menuService, InventoryStatsApiService inventoryStatsService) : Controller
{
    public async Task<IActionResult> Index(string id)
    {
        var result = await menuService.GetActiveForCurrentUserAsync();
        var group = (result.Data ?? []).FirstOrDefault(g => g.ModuleCode == id);
        if (group is null || group.Items.Count == 0)
        {
            return NotFound();
        }

        ViewData["Title"] = group.ModuleName;
        ViewData["ModuleCode"] = id;
        ViewBag.ModuleTitle = group.ModuleName;
        ViewBag.ModuleIcon = group.ModuleIcon;

        if (id == "Inventory")
        {
            var stats = await inventoryStatsService.GetAsync();
            ViewBag.InventoryStats = stats.Data;
        }

        return View(group.Items);
    }

    // Attribute-routed (distinct from "Workspace/{id}") so this doesn't get swallowed by the workspace-index
    // conventional route, which would otherwise treat "GetInventoryStats" as the {id} module code.
    [HttpGet("workspace-api/inventory-stats")]
    public async Task<IActionResult> GetInventoryStats(DateTime? dateFrom, DateTime? dateTo)
    {
        var result = await inventoryStatsService.GetAsync(dateFrom, dateTo);
        return result.Success
            ? Json(result.Data)
            : StatusCode(result.StatusCode == 0 ? 500 : result.StatusCode, new { message = result.Message });
    }
}


