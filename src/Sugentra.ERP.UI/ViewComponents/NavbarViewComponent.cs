using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models.Layout;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.ViewComponents;

/// <summary>Renders the top navbar (menu toggle + current user + logout) — current username comes from
/// the cookie claim set at login, no extra API call needed.</summary>
public class NavbarViewComponent(CompanyProfileApiService companyProfileApiService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(bool showBrand)
    {
        var username = UserClaimsPrincipal.Identity?.Name ?? "User";
        string? companyName = null;
        string? logoUrl = null;

        if (showBrand)
        {
            var identity = await companyProfileApiService.GetIdentityAsync();
            if (identity.Success)
            {
                companyName = identity.Data!.CompanyName;
                logoUrl = identity.Data!.LogoUrl;
            }
        }

        return View(new NavbarViewModel(username, showBrand, companyName, logoUrl));
    }
}
