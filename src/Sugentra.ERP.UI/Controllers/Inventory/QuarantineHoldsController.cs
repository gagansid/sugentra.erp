using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Inventory;
using Sugentra.ERP.UI.Services.Inventory;
using Sugentra.ERP.UI.Services.MasterData;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Inventory;

[Authorize(Policy = "QuarantineHold_View")]
public class QuarantineHoldsController(QuarantineHoldApiService service, ItemApiService itemService, WarehouseApiService warehouseService, BatchApiService batchService) : Controller
{
    private async Task PopulateLookupsAsync()
    {
        ViewBag.Items = (await itemService.GetAllAsync()).Data ?? [];
        ViewBag.Warehouses = (await warehouseService.GetAllAsync()).Data ?? [];
        ViewBag.Batches = (await batchService.GetAllAsync()).Data ?? [];
    }

    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, string? status = null, string? holdReason = null, long? warehouseId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<QuarantineHold>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(q =>
                q.HoldReason.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (q.Notes?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            all = all.Where(q => q.Status == status).ToList();
        }
        if (!string.IsNullOrWhiteSpace(holdReason))
        {
            all = all.Where(q => q.HoldReason == holdReason).ToList();
        }
        if (warehouseId.HasValue)
        {
            all = all.Where(q => q.WarehouseId == warehouseId.Value).ToList();
        }
        if (dateFrom.HasValue)
        {
            all = all.Where(q => q.PlacedAt.Date >= dateFrom.Value.Date).ToList();
        }
        if (dateTo.HasValue)
        {
            all = all.Where(q => q.PlacedAt.Date <= dateTo.Value.Date).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.Status = status;
        ViewBag.HoldReason = holdReason;
        ViewBag.WarehouseId = warehouseId;
        ViewBag.DateFrom = dateFrom;
        ViewBag.DateTo = dateTo;
        await PopulateLookupsAsync();
        return View(new PagedResult<QuarantineHold>
        {
            Items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "QuarantineHold_Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View(new QuarantineHold());
    }

    [HttpPost]
    [Authorize(Policy = "QuarantineHold_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuarantineHold model)
    {
        var result = await service.CreateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create quarantine hold.");
            await PopulateLookupsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Quarantine hold created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "QuarantineHold_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Quarantine hold not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "QuarantineHold_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, QuarantineHold model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update quarantine hold.");
            await PopulateLookupsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Quarantine hold updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Quarantine hold not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        return View(result.Data);
    }

    [Authorize(Policy = "QuarantineHold_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
