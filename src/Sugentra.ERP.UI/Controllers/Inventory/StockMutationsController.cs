using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Inventory;
using Sugentra.ERP.UI.Services.Inventory;
using Sugentra.ERP.UI.Services.MasterData;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Inventory;

[Authorize(Policy = "StockMutation_View")]
public class StockMutationsController(StockMutationApiService service, ItemApiService itemService, WarehouseApiService warehouseService, BatchApiService batchService) : Controller
{
    private async Task PopulateLookupsAsync()
    {
        ViewBag.Items = (await itemService.GetAllAsync()).Data ?? [];
        ViewBag.Warehouses = (await warehouseService.GetAllAsync()).Data ?? [];
        ViewBag.Batches = (await batchService.GetAllAsync()).Data ?? [];
    }

    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, string? status = null, string? mutationType = null, long? warehouseId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<StockMutationResponse>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(m =>
                m.MutationNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (m.VendorReference?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            all = all.Where(m => m.Status == status).ToList();
        }
        if (!string.IsNullOrWhiteSpace(mutationType))
        {
            all = all.Where(m => m.MutationType == mutationType).ToList();
        }
        if (warehouseId.HasValue)
        {
            all = all.Where(m => m.SourceWarehouseId == warehouseId.Value || m.DestinationWarehouseId == warehouseId.Value).ToList();
        }
        if (dateFrom.HasValue)
        {
            all = all.Where(m => m.MutationDate.Date >= dateFrom.Value.Date).ToList();
        }
        if (dateTo.HasValue)
        {
            all = all.Where(m => m.MutationDate.Date <= dateTo.Value.Date).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.Status = status;
        ViewBag.MutationType = mutationType;
        ViewBag.WarehouseId = warehouseId;
        ViewBag.DateFrom = dateFrom;
        ViewBag.DateTo = dateTo;
        await PopulateLookupsAsync();
        return View(new PagedResult<StockMutationResponse>
        {
            Items = all.OrderByDescending(m => m.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "StockMutation_Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Policy = "StockMutation_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string mutationType, long sourceWarehouseId, long? destinationWarehouseId, string? vendorReference, DateTime mutationDate, string? notes,
        List<long> itemId, List<long?> batchId, List<decimal> quantity)
    {
        if (itemId.Count != quantity.Count)
        {
            TempData["ErrorMessage"] = "Each line must have an item and quantity filled in.";
            await PopulateLookupsAsync();
            return View();
        }

        var lines = itemId.Select((id, i) => new StockMutationLineRequest(id, batchId.ElementAtOrDefault(i), quantity[i])).ToList();
        var request = new CreateStockMutationRequest(mutationType, sourceWarehouseId, destinationWarehouseId, vendorReference, mutationDate, notes, lines);
        var result = await service.CreateAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to create stock mutation.";
            await PopulateLookupsAsync();
            return View();
        }

        TempData["SuccessMessage"] = result.Message ?? "Stock mutation created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "StockMutation_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Stock mutation not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "StockMutation_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, string mutationType, long sourceWarehouseId, long? destinationWarehouseId, string? vendorReference, DateTime mutationDate, string? notes,
        List<long> itemId, List<long?> batchId, List<decimal> quantity)
    {
        if (itemId.Count != quantity.Count)
        {
            TempData["ErrorMessage"] = "Each line must have an item and quantity filled in.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var lines = itemId.Select((iid, i) => new StockMutationLineRequest(iid, batchId.ElementAtOrDefault(i), quantity[i])).ToList();
        var request = new UpdateStockMutationRequest(mutationType, sourceWarehouseId, destinationWarehouseId, vendorReference, mutationDate, notes, lines);
        var result = await service.UpdateAsync(id, request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to update stock mutation.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        TempData["SuccessMessage"] = result.Message ?? "Stock mutation updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Stock mutation not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        return View(result.Data);
    }

    [Authorize(Policy = "StockMutation_Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(long id)
    {
        var result = await service.ApproveAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [Authorize(Policy = "StockMutation_Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(long id)
    {
        var result = await service.CompleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [Authorize(Policy = "StockMutation_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
