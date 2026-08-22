using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Settings;

[Authorize(Policy = "SystemParameter_View")]
public class SystemParametersController(SystemParameterApiService service) : Controller
{
    public async Task<IActionResult> Index(string? category, string? search, bool? isActive, int page = 1, int pageSize = 10)
    {
        var request = new SystemParameterListRequest(Category: category, Search: search, IsActive: isActive, Page: page, PageSize: pageSize);
        var result = await service.GetPagedAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<SystemParameterListItemDto>());
        }

        ViewBag.Category = category;
        ViewBag.Search = search;
        ViewBag.IsActive = isActive;
        return View(result.Data);
    }

    [Authorize(Policy = "SystemParameter_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "System parameter not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "SystemParameter_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, SystemParameter model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update system parameter.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "System parameter updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
