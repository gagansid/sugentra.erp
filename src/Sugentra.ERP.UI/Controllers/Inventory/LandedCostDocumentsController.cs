using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Inventory;
using Sugentra.ERP.UI.Services.Inventory;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Inventory;

[Authorize(Policy = "Batch_View")]
public class LandedCostDocumentsController(
    LandedCostDocumentApiService service, GoodsReceiptApiService goodsReceiptService, CurrencyApiService currencyService, BatchApiService batchService) : Controller
{
    private async Task PopulateLookupsAsync()
    {
        var receipts = (await goodsReceiptService.GetAllAsync()).Data ?? [];
        // Landed cost is allocated against an already-received batch's cost, so only Posted receipts qualify.
        ViewBag.GoodsReceipts = receipts.Where(r => r.Status == "Posted").ToList();
        ViewBag.Currencies = (await currencyService.GetAllAsync()).Data ?? [];
        ViewBag.Batches = (await batchService.GetAllAsync()).Data ?? [];
    }

    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, string? status = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<LandedCostDocumentResponse>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(d => d.DocumentNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            all = all.Where(d => d.Status == status).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.Status = status;
        await PopulateLookupsAsync();
        return View(new PagedResult<LandedCostDocumentResponse>
        {
            Items = all.OrderByDescending(d => d.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "Batch_Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Policy = "Batch_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(long goodsReceiptId, string? notes,
        List<string> costType, List<decimal> amount, List<long> currencyId, List<string?> lineNotes)
    {
        if (costType.Count != amount.Count || amount.Count != currencyId.Count)
        {
            TempData["ErrorMessage"] = "Each cost line must have a cost type, amount and currency filled in.";
            await PopulateLookupsAsync();
            return View();
        }

        var lines = costType.Select((type, i) => new LandedCostDocumentLineRequest(type, amount[i], currencyId[i], lineNotes.ElementAtOrDefault(i))).ToList();
        var request = new CreateLandedCostDocumentRequest(goodsReceiptId, "ByValue", notes, lines);
        var result = await service.CreateAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to create landed cost document.";
            await PopulateLookupsAsync();
            return View();
        }

        TempData["SuccessMessage"] = result.Message ?? "Landed cost document created successfully.";
        return RedirectToAction(nameof(Detail), new { id = result.Data!.Id });
    }

    public async Task<IActionResult> Detail(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Landed cost document not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "Batch_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(long id)
    {
        var result = await service.PostDocumentAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }
}
