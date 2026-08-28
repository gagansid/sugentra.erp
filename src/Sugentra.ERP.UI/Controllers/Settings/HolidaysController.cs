using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Settings;

[Authorize(Policy = "Holiday_View")]
public class HolidaysController(HolidayApiService service) : Controller
{
    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<Holiday>());
        }

        var all = (result.Data ?? []).OrderBy(h => h.Date).AsEnumerable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(h => h.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        var list = all.ToList();
        ViewBag.Keyword = keyword;
        return View(new PagedResult<Holiday>
        {
            Items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = list.Count
        });
    }

    [Authorize(Policy = "Holiday_Create")]
    public IActionResult Create() => View(new Holiday { Date = DateTime.Today });

    [HttpPost]
    [Authorize(Policy = "Holiday_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Holiday model)
    {
        var result = await service.CreateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create holiday.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Holiday created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Holiday_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Holiday not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "Holiday_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Holiday model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update holiday.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Holiday updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Holiday_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
