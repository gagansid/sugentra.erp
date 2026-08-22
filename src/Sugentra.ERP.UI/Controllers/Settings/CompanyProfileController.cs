using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Settings;

[Authorize(Policy = "CompanyProfile_View")]
public class CompanyProfileController(CompanyProfileApiService service, UploadApiService uploadService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var result = await service.GetAsync();
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Company profile not configured.";
            return View(new CompanyProfile());
        }

        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "CompanyProfile_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CompanyProfile model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await service.UpdateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update company profile.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Company profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // AJAX-only: uploads a logo/icon image, returns its URL so the form can show a preview before Save is clicked.
    [HttpPost]
    [Authorize(Policy = "CompanyProfile_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImage(IFormFile file, string? currentUrl = null)
    {
        var result = await uploadService.UploadAsync(file, "company-logos", currentUrl);
        if (!result.Success || result.Data is null)
        {
            return BadRequest(new { message = result.Message ?? "Upload failed." });
        }

        return Ok(new { url = result.Data.Url });
    }
}
