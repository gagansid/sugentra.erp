using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Inventory;
using Sugentra.ERP.UI.Services.Inventory;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Inventory;

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

    [Authorize(Policy = "Batch_Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View(new LandedCostAllocation());
    }

    [HttpPost]
    [Authorize(Policy = "Batch_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LandedCostAllocation model)
    {
        var result = await service.CreateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create landed cost allocation.");
            await PopulateLookupsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Landed cost allocation created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Batch_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Landed cost allocation not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "Batch_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, LandedCostAllocation model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update landed cost allocation.");
            await PopulateLookupsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Landed cost allocation updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Batch_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
