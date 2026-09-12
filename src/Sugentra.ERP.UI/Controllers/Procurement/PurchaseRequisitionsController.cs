using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Procurement;
using Sugentra.ERP.UI.Services.MasterData;
using Sugentra.ERP.UI.Services.Procurement;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Procurement;

[Authorize(Policy = "PurchaseRequisition_View")]
public class PurchaseRequisitionsController(PurchaseRequisitionApiService service, ItemApiService itemService, WarehouseApiService warehouseService) : Controller
{
    private async Task PopulateLookupsAsync()
    {
        ViewBag.Items = (await itemService.GetAllAsync()).Data ?? [];
        ViewBag.Warehouses = (await warehouseService.GetAllAsync()).Data ?? [];
    }

    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, string? status = null,
        string? number = null, DateTime? dateFrom = null, DateTime? dateTo = null, string? linked = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<PurchaseRequisitionResponse>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(r => r.RequisitionNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(number))
        {
            all = all.Where(r => r.RequisitionNumber.Contains(number, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (dateFrom.HasValue)
        {
            all = all.Where(r => r.RequisitionDate.Date >= dateFrom.Value.Date).ToList();
        }
        if (dateTo.HasValue)
        {
            all = all.Where(r => r.RequisitionDate.Date <= dateTo.Value.Date).ToList();
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            all = all.Where(r => r.Status == status).ToList();
        }
        if (linked == "Ordered")
        {
            all = all.Where(r => r.LinkedOrders.Count > 0).ToList();
        }
        else if (linked == "NotOrdered")
        {
            all = all.Where(r => r.LinkedOrders.Count == 0).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.Status = status;
        ViewBag.Number = number;
        ViewBag.DateFrom = dateFrom;
        ViewBag.DateTo = dateTo;
        ViewBag.Linked = linked;
        return View(new PagedResult<PurchaseRequisitionResponse>
        {
            Items = all.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "PurchaseRequisition_Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Policy = "PurchaseRequisition_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(long warehouseId, DateTime requisitionDate, string? notes,
        List<long> itemId, List<decimal> quantity, List<string?> lineNotes)
    {
        if (itemId.Count != quantity.Count)
        {
            TempData["ErrorMessage"] = "Each line must have an item and quantity filled in.";
            await PopulateLookupsAsync();
            return View();
        }

        var lines = itemId.Select((id, i) => new PurchaseRequisitionLineRequest(id, quantity[i], lineNotes.ElementAtOrDefault(i))).ToList();
        var request = new CreatePurchaseRequisitionRequest(warehouseId, requisitionDate, notes, lines);
        var result = await service.CreateAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to create purchase requisition.";
            await PopulateLookupsAsync();
            return View();
        }

        TempData["SuccessMessage"] = result.Message ?? "Purchase requisition created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "PurchaseRequisition_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Purchase requisition not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "PurchaseRequisition_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, long warehouseId, DateTime requisitionDate, string? notes,
        List<long> itemId, List<decimal> quantity, List<string?> lineNotes)
    {
        if (itemId.Count != quantity.Count)
        {
            TempData["ErrorMessage"] = "Each line must have an item and quantity filled in.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var lines = itemId.Select((iid, i) => new PurchaseRequisitionLineRequest(iid, quantity[i], lineNotes.ElementAtOrDefault(i))).ToList();
        var request = new UpdatePurchaseRequisitionRequest(warehouseId, requisitionDate, notes, lines);
        var result = await service.UpdateAsync(id, request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to update purchase requisition.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        TempData["SuccessMessage"] = result.Message ?? "Purchase requisition updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Purchase requisition not found.";
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

    [Authorize(Policy = "PurchaseRequisition_Approve")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(long id)
    {
        var result = await service.SubmitAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [Authorize(Policy = "PurchaseRequisition_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
