using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Settings;

[Authorize(Policy = "Menu_View")]
public class MenusController(MenuApiService service, ModuleApiService moduleService) : Controller
{
    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<Menu>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(m =>
                (m.ModuleName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.Name?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.Controller?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.Action?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.RequiredPermission?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.IsActive ? "Active" : "Inactive").Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        ViewBag.Keyword = keyword;
        return View(new PagedResult<Menu>
        {
            Items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "Menu_Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View(new Menu());
    }

    [HttpPost]
    [Authorize(Policy = "Menu_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Menu model)
    {
        var result = await service.CreateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create menu.");
            await PopulateDropdownsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Menu created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Menu_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Menu not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdownsAsync(excludeMenuId: id);
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "Menu_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Menu model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update menu.");
            await PopulateDropdownsAsync(excludeMenuId: id);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Menu updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Menu_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // A menu can't be its own parent, or a descendant's parent (kept simple: just excludes itself here;
    // deeper cycle prevention would need a recursive check if nesting grows beyond 2 levels).
    private async Task PopulateDropdownsAsync(long? excludeMenuId = null)
    {
        var modules = await moduleService.GetAllAsync();
        ViewBag.Modules = modules.Data ?? [];

        var menus = await service.GetAllAsync();
        ViewBag.ParentCandidates = (menus.Data ?? []).Where(m => m.Id != excludeMenuId).ToList();
    }
}
