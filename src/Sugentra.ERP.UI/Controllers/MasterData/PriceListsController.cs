using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.MasterData;
using Sugentra.ERP.UI.Services.MasterData;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.MasterData;

[Authorize(Policy = "MasterData_View")]
public class PriceListsController(
    PriceListApiService service, ItemApiService itemService, CurrencyApiService currencyService, BusinessPartnerApiService partnerService) : Controller
{
    private async Task PopulateLookupsAsync()
    {
        var items = await itemService.GetAllAsync();
        var currencies = await currencyService.GetAllAsync();
        var partners = await partnerService.GetAllAsync();
        ViewBag.Items = items.Data ?? [];
        ViewBag.Currencies = currencies.Data ?? [];
        ViewBag.BusinessPartners = partners.Data ?? [];
    }

    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, bool? isActive = null)
    {
        var result = await service.GetPagedAsync(new PriceListListRequest(keyword, page, pageSize, isActive));
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<PriceListListItemDto>());
        }

        ViewBag.Keyword = keyword;
        ViewBag.IsActive = isActive;
        return View(result.Data);
    }

    [Authorize(Policy = "MasterData_Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Policy = "MasterData_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string type, long currencyId, long? businessPartnerId, List<long> itemId, List<decimal> price, List<DateTime> effectiveDate)
    {
        if (itemId.Count != price.Count || itemId.Count != effectiveDate.Count)
        {
            TempData["ErrorMessage"] = "Each item line must have a price and effective date filled in.";
            await PopulateLookupsAsync();
            return View();
        }

        var lines = itemId.Select((id, i) => new PriceListLineRequest(id, price[i], effectiveDate[i])).ToList();
        var request = new CreatePriceListRequest(name, type, currencyId, businessPartnerId, lines);
        var result = await service.CreateAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to create price list.";
            await PopulateLookupsAsync();
            return View();
        }

        TempData["SuccessMessage"] = result.Message ?? "Price list created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "MasterData_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Price list not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "MasterData_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, string name, string type, long currencyId, long? businessPartnerId, bool isActive, List<long> itemId, List<decimal> price, List<DateTime> effectiveDate)
    {
        if (itemId.Count != price.Count || itemId.Count != effectiveDate.Count)
        {
            TempData["ErrorMessage"] = "Each item line must have a price and effective date filled in.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var lines = itemId.Select((iid, i) => new PriceListLineRequest(iid, price[i], effectiveDate[i])).ToList();
        var request = new UpdatePriceListRequest(name, type, currencyId, businessPartnerId, isActive, lines);
        var result = await service.UpdateAsync(id, request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to update price list.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        TempData["SuccessMessage"] = result.Message ?? "Price list updated successfully.";
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
            TempData["ErrorMessage"] = result.Message ?? "Price list not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        var adjacentResult = await service.GetAdjacentAsync(id);
        ViewBag.PreviousId = adjacentResult.Data?.PreviousId;
        ViewBag.NextId = adjacentResult.Data?.NextId;
        ViewBag.FirstId = adjacentResult.Data?.FirstId;
        ViewBag.LastId = adjacentResult.Data?.LastId;
        return View(result.Data);
    }
}
