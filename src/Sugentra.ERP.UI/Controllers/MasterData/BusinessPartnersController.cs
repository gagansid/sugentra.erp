using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Auth;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.MasterData;
using Sugentra.ERP.UI.Services;
using Sugentra.ERP.UI.Services.MasterData;

namespace Sugentra.ERP.UI.Controllers.MasterData;

[Authorize(Policy = "MasterData_View")]
public class BusinessPartnersController(BusinessPartnerApiService service, UploadApiService uploadService) : Controller
{
    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, bool? isActive = null)
    {
        var result = await service.GetPagedAsync(new BusinessPartnerListRequest(keyword, page, pageSize, isActive));
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<BusinessPartnerListItemDto>());
        }

        ViewBag.Keyword = keyword;
        ViewBag.IsActive = isActive;
        return View(result.Data);
    }

    [Authorize(Policy = "MasterData_Create")]
    public IActionResult Create() => View();

    [HttpPost]
    [Authorize(Policy = "MasterData_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string code, string name, string partnerType, bool isActive,
        string? taxId, string? taxRegisteredName, string? taxAddress, string? nik, string? taxpayerType,
        string? nitku, bool isPkp, string? sktNumber, string? kluCode, string? logoUrl,
        List<string> addressType, List<string> address, List<string?> addressCity, List<string?> addressProvince, List<string?> addressPostalCode, List<string?> addressCountry, List<bool> addressIsPrimary,
        List<string> contactType, List<string?> contactName, List<string> contactValue, List<bool> contactIsPrimary)
    {
        var addresses = address
            .Select((a, i) => new BusinessPartnerAddressRequest(
                addressType.ElementAtOrDefault(i) ?? "Office", a,
                addressCity.ElementAtOrDefault(i), addressProvince.ElementAtOrDefault(i),
                addressPostalCode.ElementAtOrDefault(i), addressCountry.ElementAtOrDefault(i), addressIsPrimary.ElementAtOrDefault(i)))
            .Where(a => !string.IsNullOrWhiteSpace(a.Address))
            .ToList();

        var contacts = contactValue
            .Select((v, i) => new BusinessPartnerContactRequest(contactType.ElementAtOrDefault(i) ?? "Phone", contactName.ElementAtOrDefault(i), v, contactIsPrimary.ElementAtOrDefault(i)))
            .Where(c => !string.IsNullOrWhiteSpace(c.Value))
            .ToList();

        var request = new CreateBusinessPartnerRequest(
            code, name, partnerType, isActive,
            taxId, taxRegisteredName, taxAddress, nik, taxpayerType,
            nitku, isPkp, sktNumber, kluCode, logoUrl, addresses, contacts);

        var result = await service.CreateAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to create business partner.";
            return View();
        }

        TempData["SuccessMessage"] = result.Message ?? "Business partner created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "MasterData_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Business partner not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "MasterData_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        long id, string name, string partnerType, bool isActive,
        string? taxId, string? taxRegisteredName, string? taxAddress, string? nik, string? taxpayerType,
        string? nitku, bool isPkp, string? sktNumber, string? kluCode, string? logoUrl,
        List<string> addressType, List<string> address, List<string?> addressCity, List<string?> addressProvince, List<string?> addressPostalCode, List<string?> addressCountry, List<bool> addressIsPrimary,
        List<string> contactType, List<string?> contactName, List<string> contactValue, List<bool> contactIsPrimary)
    {
        var addresses = address
            .Select((a, i) => new BusinessPartnerAddressRequest(
                addressType.ElementAtOrDefault(i) ?? "Office", a,
                addressCity.ElementAtOrDefault(i), addressProvince.ElementAtOrDefault(i),
                addressPostalCode.ElementAtOrDefault(i), addressCountry.ElementAtOrDefault(i), addressIsPrimary.ElementAtOrDefault(i)))
            .Where(a => !string.IsNullOrWhiteSpace(a.Address))
            .ToList();

        var contacts = contactValue
            .Select((v, i) => new BusinessPartnerContactRequest(contactType.ElementAtOrDefault(i) ?? "Phone", contactName.ElementAtOrDefault(i), v, contactIsPrimary.ElementAtOrDefault(i)))
            .Where(c => !string.IsNullOrWhiteSpace(c.Value))
            .ToList();

        var request = new UpdateBusinessPartnerRequest(
            name, partnerType, isActive,
            taxId, taxRegisteredName, taxAddress, nik, taxpayerType,
            nitku, isPkp, sktNumber, kluCode, logoUrl, addresses, contacts);

        var result = await service.UpdateAsync(id, request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to update business partner.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        TempData["SuccessMessage"] = result.Message ?? "Business partner updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "MasterData_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Business partner not found.";
            return RedirectToAction(nameof(Index));
        }

        var adjacentResult = await service.GetAdjacentAsync(id);
        ViewBag.PreviousId = adjacentResult.Data?.PreviousId;
        ViewBag.NextId = adjacentResult.Data?.NextId;
        ViewBag.FirstId = adjacentResult.Data?.FirstId;
        ViewBag.LastId = adjacentResult.Data?.LastId;
        return View(result.Data);
    }

    // AJAX-only: uploads a logo image, returns its URL so the form can show a preview before Save is clicked.
    // Used from both the Create and Edit forms, so either permission is accepted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadLogo(IFormFile file, string? currentUrl = null)
    {
        if (!User.HasPermission("MasterData_Create") && !User.HasPermission("MasterData_Edit"))
        {
            return Forbid();
        }

        var result = await uploadService.UploadAsync(file, "business-partner-logos", currentUrl);
        if (!result.Success || result.Data is null)
        {
            return BadRequest(new { message = result.Message ?? "Upload failed." });
        }

        return Ok(new { url = result.Data.Url });
    }
}

