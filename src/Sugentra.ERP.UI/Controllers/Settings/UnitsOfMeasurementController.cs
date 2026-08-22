using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Settings;

[Authorize(Policy = "UnitOfMeasurement_View")]
public class UnitsOfMeasurementController(UnitOfMeasurementApiService service) : Controller
{
    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, bool? isActive = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<UnitOfMeasurement>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(u =>
                (u.Code?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.Name?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.IsActive ? "Active" : "Inactive").Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (isActive.HasValue)
        {
            all = all.Where(u => u.IsActive == isActive.Value).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.IsActive = isActive;
        return View(new PagedResult<UnitOfMeasurement>
        {
            Items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "UnitOfMeasurement_Create")]
    public IActionResult Create() => View(new UnitOfMeasurement());

    [HttpPost]
    [Authorize(Policy = "UnitOfMeasurement_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UnitOfMeasurement model)
    {
        var result = await service.CreateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create unit of measurement.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Unit of measurement created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "UnitOfMeasurement_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Unit of measurement not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "UnitOfMeasurement_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, UnitOfMeasurement model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update unit of measurement.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Unit of measurement updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "UnitOfMeasurement_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
