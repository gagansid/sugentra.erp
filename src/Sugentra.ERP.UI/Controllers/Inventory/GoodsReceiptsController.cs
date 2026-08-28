using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Inventory;
using Sugentra.ERP.UI.Services.Inventory;
using Sugentra.ERP.UI.Services.MasterData;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Inventory;

[Authorize(Policy = "GoodsReceipt_View")]
public class GoodsReceiptsController(GoodsReceiptApiService service, ItemApiService itemService, WarehouseApiService warehouseService, BatchApiService batchService) : Controller
{
    private async Task PopulateLookupsAsync()
    {
        ViewBag.Items = (await itemService.GetAllAsync()).Data ?? [];
        ViewBag.Warehouses = (await warehouseService.GetAllAsync()).Data ?? [];
        ViewBag.Batches = (await batchService.GetAllAsync()).Data ?? [];
    }

    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, string? status = null,
        string? number = null, string? vendorReference = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<GoodsReceiptResponse>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(r =>
                r.ReceiptNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (r.VendorReference?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
        if (!string.IsNullOrWhiteSpace(number))
        {
            all = all.Where(r => r.ReceiptNumber.Contains(number, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(vendorReference))
        {
            all = all.Where(r => r.VendorReference?.Contains(vendorReference, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
        }
        if (dateFrom.HasValue)
        {
            all = all.Where(r => r.ReceiptDate.Date >= dateFrom.Value.Date).ToList();
        }
        if (dateTo.HasValue)
        {
            all = all.Where(r => r.ReceiptDate.Date <= dateTo.Value.Date).ToList();
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            all = all.Where(r => r.Status == status).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.Status = status;
        ViewBag.Number = number;
        ViewBag.VendorReference = vendorReference;
        ViewBag.DateFrom = dateFrom;
        ViewBag.DateTo = dateTo;
        await PopulateLookupsAsync();
        return View(new PagedResult<GoodsReceiptResponse>
        {
            Items = all.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "GoodsReceipt_Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Policy = "GoodsReceipt_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(long warehouseId, string? vendorReference, DateTime receiptDate, string? notes,
        List<long> itemId, List<long> batchId, List<decimal> quantity, List<decimal> unitCost)
    {
        if (itemId.Count != quantity.Count)
        {
            TempData["ErrorMessage"] = "Each line must have an item and quantity filled in.";
            await PopulateLookupsAsync();
            return View();
        }

        var lines = itemId.Select((id, i) => new GoodsReceiptLineRequest(id, batchId.ElementAtOrDefault(i), quantity[i], unitCost.ElementAtOrDefault(i))).ToList();
        var request = new CreateGoodsReceiptRequest(warehouseId, vendorReference, receiptDate, notes, lines);
        var result = await service.CreateAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to create goods receipt.";
            await PopulateLookupsAsync();
            return View();
        }

        TempData["SuccessMessage"] = result.Message ?? "Goods receipt created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "GoodsReceipt_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Goods receipt not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "GoodsReceipt_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, long warehouseId, string? vendorReference, DateTime receiptDate, string? notes,
        List<long> itemId, List<long> batchId, List<decimal> quantity, List<decimal> unitCost)
    {
        if (itemId.Count != quantity.Count)
        {
            TempData["ErrorMessage"] = "Each line must have an item and quantity filled in.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var lines = itemId.Select((iid, i) => new GoodsReceiptLineRequest(iid, batchId.ElementAtOrDefault(i), quantity[i], unitCost.ElementAtOrDefault(i))).ToList();
        var request = new UpdateGoodsReceiptRequest(warehouseId, vendorReference, receiptDate, notes, lines);
        var result = await service.UpdateAsync(id, request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to update goods receipt.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        TempData["SuccessMessage"] = result.Message ?? "Goods receipt updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Goods receipt not found.";
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

    [Authorize(Policy = "GoodsReceipt_Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(long id)
    {
        var result = await service.PostGoodsReceiptAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [Authorize(Policy = "GoodsReceipt_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
