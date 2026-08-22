using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Settings;

[Authorize(Policy = "Currency_View")]
public class CurrenciesController(CurrencyApiService service) : Controller
{
    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, bool? isActive = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<Currency>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(c =>
                (c.Code?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.Name?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.Symbol?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.IsActive ? "Active" : "Inactive").Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (isActive.HasValue)
        {
            all = all.Where(c => c.IsActive == isActive.Value).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.IsActive = isActive;
        return View(new PagedResult<Currency>
        {
            Items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "Currency_Create")]
    public IActionResult Create() => View(new Currency());

    [HttpPost]
    [Authorize(Policy = "Currency_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Currency model)
    {
        var result = await service.CreateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create currency.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Currency created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Currency_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Currency not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "Currency_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Currency model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update currency.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Currency updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Currency_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
