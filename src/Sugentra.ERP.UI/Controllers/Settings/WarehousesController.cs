using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Settings;

[Authorize(Policy = "Warehouse_View")]
public class WarehousesController(WarehouseApiService service) : Controller
{
    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, bool? isActive = null, string? warehouseType = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<Warehouse>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(w =>
                (w.Code?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (w.Name?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (w.City?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (w.IsActive ? "Active" : "Inactive").Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (isActive.HasValue)
        {
            all = all.Where(w => w.IsActive == isActive.Value).ToList();
        }
        if (!string.IsNullOrWhiteSpace(warehouseType))
        {
            all = all.Where(w => w.WarehouseType == warehouseType).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.IsActive = isActive;
        ViewBag.WarehouseType = warehouseType;
        return View(new PagedResult<Warehouse>
        {
            Items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "Warehouse_Create")]
    public IActionResult Create() => View(new Warehouse());

    [HttpPost]
    [Authorize(Policy = "Warehouse_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Warehouse model)
    {
        var result = await service.CreateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create warehouse.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Warehouse created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Warehouse_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Warehouse not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "Warehouse_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Warehouse model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update warehouse.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Warehouse updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Warehouse_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
