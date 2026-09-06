using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Inventory;
using Sugentra.ERP.UI.Services.Inventory;
using Sugentra.ERP.UI.Services.MasterData;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Inventory;

[Authorize(Policy = "StockOpname_View")]
public class StockOpnamesController(StockOpnameApiService service, ItemApiService itemService, WarehouseApiService warehouseService, BatchApiService batchService) : Controller
{
    private async Task PopulateLookupsAsync()
    {
        ViewBag.Items = (await itemService.GetAllAsync()).Data ?? [];
        ViewBag.Warehouses = (await warehouseService.GetAllAsync()).Data ?? [];
        ViewBag.Batches = (await batchService.GetAllAsync()).Data ?? [];
    }

    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, string? status = null, long? warehouseId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<StockOpnameResponse>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(o => o.OpnameNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            all = all.Where(o => o.Status == status).ToList();
        }
        if (warehouseId.HasValue)
        {
            all = all.Where(o => o.WarehouseId == warehouseId.Value).ToList();
        }
        if (dateFrom.HasValue)
        {
            all = all.Where(o => o.OpnameDate.Date >= dateFrom.Value.Date).ToList();
        }
        if (dateTo.HasValue)
        {
            all = all.Where(o => o.OpnameDate.Date <= dateTo.Value.Date).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.Status = status;
        ViewBag.WarehouseId = warehouseId;
        ViewBag.DateFrom = dateFrom;
        ViewBag.DateTo = dateTo;
        await PopulateLookupsAsync();
        return View(new PagedResult<StockOpnameResponse>
        {
            Items = all.OrderByDescending(o => o.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "StockOpname_Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Policy = "StockOpname_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(long warehouseId, DateTime opnameDate, string? notes,
        List<long> itemId, List<long?> batchId, List<decimal> systemQuantity, List<decimal> countedQuantity, List<string?> lineNotes)
    {
        if (itemId.Count != systemQuantity.Count || itemId.Count != countedQuantity.Count)
        {
            TempData["ErrorMessage"] = "Each line must have an item, system quantity and counted quantity filled in.";
            await PopulateLookupsAsync();
            return View();
        }

        var lines = itemId.Select((id, i) => new StockOpnameLineRequest(id, batchId.ElementAtOrDefault(i), systemQuantity[i], countedQuantity[i], lineNotes.ElementAtOrDefault(i))).ToList();
        var request = new CreateStockOpnameRequest(warehouseId, opnameDate, notes, lines);
        var result = await service.CreateAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to create stock opname.";
            await PopulateLookupsAsync();
            return View();
        }

        TempData["SuccessMessage"] = result.Message ?? "Stock opname created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "StockOpname_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Stock opname not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "StockOpname_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, DateTime opnameDate, string? notes,
        List<long> itemId, List<long?> batchId, List<decimal> systemQuantity, List<decimal> countedQuantity, List<string?> lineNotes)
    {
        if (itemId.Count != systemQuantity.Count || itemId.Count != countedQuantity.Count)
        {
            TempData["ErrorMessage"] = "Each line must have an item, system quantity and counted quantity filled in.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var lines = itemId.Select((iid, i) => new StockOpnameLineRequest(iid, batchId.ElementAtOrDefault(i), systemQuantity[i], countedQuantity[i], lineNotes.ElementAtOrDefault(i))).ToList();
        var request = new UpdateStockOpnameRequest(opnameDate, notes, lines);
        var result = await service.UpdateAsync(id, request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to update stock opname.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        TempData["SuccessMessage"] = result.Message ?? "Stock opname updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Stock opname not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        var adjacentResult = await service.GetAdjacentAsync(id);
        ViewBag.PreviousId = adjacentResult.Data?.PreviousId;
        ViewBag.NextId = adjacentResult.Data?.NextId;
        ViewBag.FirstId = adjacentResult.Data?.FirstId;
        ViewBag.LastId = adjacentResult.Data?.LastId;
        var historyResult = await service.GetApprovalHistoryAsync(id);
        ViewBag.ApprovalHistory = historyResult.Data ?? [];
        return View(result.Data);
    }

    [Authorize(Policy = "StockOpname_Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(long id)
    {
        var result = await service.PostStockOpnameAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [Authorize(Policy = "StockOpname_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
