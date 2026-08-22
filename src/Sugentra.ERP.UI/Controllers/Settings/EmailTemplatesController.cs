using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Settings;

[Authorize(Policy = "EmailTemplate_View")]
public class EmailTemplatesController(EmailTemplateApiService service, EmailTemplateParameterApiService parameterService, SystemParameterApiService systemParameterService, ParamFormatOptionApiService formatOptionService) : Controller
{
    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<EmailTemplate>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(t =>
                (t.Code?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.Name?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.FromEmail?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.Subject?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.IsActive ? "Active" : "Inactive").Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        ViewBag.Keyword = keyword;
        return View(new PagedResult<EmailTemplate>
        {
            Items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "EmailTemplate_Create")]
    public IActionResult Create() => View(new EmailTemplate());

    [HttpPost]
    [Authorize(Policy = "EmailTemplate_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmailTemplate model)
    {
        var result = await service.CreateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create email template.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Email template created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "EmailTemplate_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Email template not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "EmailTemplate_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, EmailTemplate model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update email template.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Email template updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "EmailTemplate_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetParameters(long emailTemplateId)
    {
        var result = await parameterService.GetByTemplateIdAsync(emailTemplateId);
        return Json(new { success = result.Success, message = result.Message, data = result.Data });
    }

    [HttpGet]
    public async Task<IActionResult> GetSystemParameterOptions(string category)
    {
        var result = await systemParameterService.GetByCategoryAsync(category);
        return Json(new { success = result.Success, message = result.Message, data = result.Data });
    }

    [HttpGet]
    public async Task<IActionResult> GetParamFormatOptions(string dataType)
    {
        var result = await formatOptionService.GetByDataTypeAsync(dataType);
        return Json(new { success = result.Success, message = result.Message, data = result.Data });
    }

    [HttpPost]
    [Authorize(Policy = "EmailTemplateParameter_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveParameter(Models.Settings.EmailTemplateParameter model)
    {
        var result = model.Id > 0
            ? await parameterService.UpdateAsync(model.Id, model)
            : await parameterService.CreateAsync(model);
        return Json(new { success = result.Success, message = result.Message, data = result.Data });
    }

    [HttpPost]
    [Authorize(Policy = "EmailTemplateParameter_Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteParameter(long id)
    {
        var result = await parameterService.DeleteAsync(id);
        return Json(new { success = result.Success, message = result.Message });
    }
}
