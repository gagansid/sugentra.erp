using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Inventory;
using Sugentra.ERP.UI.Services.Inventory;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Inventory;

// Read-only: allocations are now computed by posting a LandedCostDocument (see LandedCostDocumentsController).
[Authorize(Policy = "Batch_View")]
public class LandedCostAllocationsController(LandedCostAllocationApiService service, BatchApiService batchService, CurrencyApiService currencyService) : Controller
{
    private async Task PopulateLookupsAsync()
    {
        ViewBag.Batches = (await batchService.GetAllAsync()).Data ?? [];
        ViewBag.Currencies = (await currencyService.GetAllAsync()).Data ?? [];
    }

    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, long? batchId = null, string? costType = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<LandedCostAllocation>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(a => a.Notes?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
        }
        if (batchId.HasValue)
        {
            all = all.Where(a => a.BatchId == batchId.Value).ToList();
        }
        if (!string.IsNullOrWhiteSpace(costType))
        {
            all = all.Where(a => a.CostType == costType).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.BatchId = batchId;
        ViewBag.CostType = costType;
        await PopulateLookupsAsync();
        return View(new PagedResult<LandedCostAllocation>
        {
            Items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }
}

