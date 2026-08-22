using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers;

public class DashboardController(ModuleApiService moduleApiService) : Controller
{
    public IActionResult Index() => View();

    // AJAX endpoint: proxies to the API, called via fetch() from Index.cshtml so the browser's Network tab
    // shows the request/response directly (see docs/plan.md's UI navigation decision for why).
    [HttpGet]
    public async Task<IActionResult> GetActiveModules()
    {
        var result = await moduleApiService.GetActiveForCurrentUserAsync();
        return result.Success
            ? Json(result.Data)
            : StatusCode(result.StatusCode == 0 ? 500 : result.StatusCode, new { message = result.Message });
    }
}
