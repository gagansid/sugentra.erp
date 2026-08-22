using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Settings;

[Authorize(Policy = "EmailSetting_View")]
public class EmailSettingsController(EmailSettingApiService service) : Controller
{
    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<EmailSetting>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(s =>
                (s.Name?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (s.Provider?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (s.Host?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                s.Port.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (s.ConnectionSecurity?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (s.Username?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (s.IsActive ? "Active" : "Inactive").Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        ViewBag.Keyword = keyword;
        return View(new PagedResult<EmailSetting>
        {
            Items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "EmailSetting_Create")]
    public IActionResult Create() => View(new EmailSetting());

    [HttpPost]
    [Authorize(Policy = "EmailSetting_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmailSetting model)
    {
        var result = await service.CreateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create email setting.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Email setting created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "EmailSetting_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Email setting not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "EmailSetting_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, EmailSetting model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update email setting.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Email setting updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "EmailSetting_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "EmailSetting_Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Test(EmailSetting model, string toEmail, string? subject, string? body)
    {
        var result = await service.TestAsync(model, toEmail, subject, body);
        return Json(new { success = result.Success, message = result.Message, errors = result.Errors });
    }
}
