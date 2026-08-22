using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.ViewComponents;

/// <summary>Renders the vertical sidebar menu from Setting_Menus, showing only the modules/items the
/// current user has permission to see (server calls /api/settings/menus/active, filtered by their JWT
/// "permission" claims - already permission-scoped, no extra client-side filtering needed here).</summary>
public class SidebarMenuViewComponent(MenuApiService menuService, CompanyProfileApiService companyProfileApiService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var result = await menuService.GetActiveForCurrentUserAsync();

        var identity = await companyProfileApiService.GetIdentityAsync();
        ViewBag.CompanyName = identity.Success ? identity.Data?.CompanyName : null;
        ViewBag.LogoUrl = identity.Success ? identity.Data?.LogoUrl : null;

        return View(result.Data ?? []);
    }
}
