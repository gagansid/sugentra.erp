using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Settings;

[Authorize(Policy = "DocumentNumbering_View")]
public class DocumentNumberingsController(DocumentNumberingApiService service) : Controller
{
    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<DocumentNumbering>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(d =>
                d.DocumentType.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (d.Prefix?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (d.Suffix?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        ViewBag.Keyword = keyword;
        return View(new PagedResult<DocumentNumbering>
        {
            Items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "DocumentNumbering_Create")]
    public IActionResult Create() => View(new DocumentNumbering());

    [HttpPost]
    [Authorize(Policy = "DocumentNumbering_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentNumbering model)
    {
        var result = await service.CreateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create document numbering config.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Document numbering config created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "DocumentNumbering_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Document numbering config not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "DocumentNumbering_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, DocumentNumbering model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update document numbering config.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Document numbering config updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "DocumentNumbering_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
