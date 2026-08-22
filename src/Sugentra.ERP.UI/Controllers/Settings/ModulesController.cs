using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Settings;

[Authorize(Policy = "Module_View")]
public class ModulesController(ModuleApiService service) : Controller
{
    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<Module>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(m =>
                (m.Code?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.Name?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.Icon?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.IsActive ? "Active" : "Inactive").Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        ViewBag.Keyword = keyword;
        return View(new PagedResult<Module>
        {
            Items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "Module_Create")]
    public IActionResult Create() => View(new Module());

    [HttpPost]
    [Authorize(Policy = "Module_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Module model)
    {
        var result = await service.CreateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create module.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Module created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Module_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Module not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "Module_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Module model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update module.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Module updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Module_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
