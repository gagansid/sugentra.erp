using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers;

// Landing page for a dashboard module tile (Setting_Modules.Route points here) - shows that module's own
// sub-pages as cards before the user drills into any specific one, and puts the sidebar into module-scoped mode.
public class WorkspaceController(MenuApiService menuService) : Controller
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
        return View(group.Items);
    }
}
