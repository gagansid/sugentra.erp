using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Procurement;
using Sugentra.ERP.UI.Services.MasterData;
using Sugentra.ERP.UI.Services.Procurement;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Procurement;

[Authorize(Policy = "PurchaseOrder_View")]
public class PurchaseOrdersController(
    PurchaseOrderApiService service, PurchaseRequisitionApiService requisitionService, BusinessPartnerApiService businessPartnerService,
    CurrencyApiService currencyService, ItemApiService itemService, WarehouseApiService warehouseService) : Controller
{
    private async Task PopulateLookupsAsync()
    {
        var vendors = (await businessPartnerService.GetAllAsync()).Data ?? [];
        ViewBag.Vendors = vendors.Where(v => v.PartnerType == "Supplier" || v.PartnerType == "Both").ToList();
        ViewBag.Currencies = (await currencyService.GetAllAsync()).Data ?? [];
        ViewBag.Items = (await itemService.GetAllAsync()).Data ?? [];
        ViewBag.Warehouses = (await warehouseService.GetAllAsync()).Data ?? [];

        // A PR may be split across multiple POs, so every Approved requisition stays selectable regardless of prior use.
        var requisitions = (await requisitionService.GetAllAsync()).Data ?? [];
        ViewBag.ApprovedRequisitions = requisitions.Where(r => r.Status == "Approved").ToList();
    }

    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, string? status = null,
        string? number = null, DateTime? dateFrom = null, DateTime? dateTo = null, string? sourced = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<PurchaseOrderResponse>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(o =>
                o.OrderNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (o.VendorName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
        if (!string.IsNullOrWhiteSpace(number))
        {
            all = all.Where(o => o.OrderNumber.Contains(number, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (dateFrom.HasValue)
        {
            all = all.Where(o => o.OrderDate.Date >= dateFrom.Value.Date).ToList();
        }
        if (dateTo.HasValue)
        {
            all = all.Where(o => o.OrderDate.Date <= dateTo.Value.Date).ToList();
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            all = all.Where(o => o.Status == status).ToList();
        }
        if (sourced == "FromRequisition")
        {
            all = all.Where(o => o.SourceRequisitions.Count > 0).ToList();
        }
        else if (sourced == "Manual")
        {
            all = all.Where(o => o.SourceRequisitions.Count == 0).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.Status = status;
        ViewBag.Number = number;
        ViewBag.DateFrom = dateFrom;
        ViewBag.DateTo = dateTo;
        ViewBag.Sourced = sourced;
        return View(new PagedResult<PurchaseOrderResponse>
        {
            Items = all.OrderByDescending(o => o.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    [Authorize(Policy = "PurchaseOrder_Create")]
    public async Task<IActionResult> Create(long? purchaseRequisitionId = null)
    {
        await PopulateLookupsAsync();
        ViewBag.SelectedRequisitionIds = purchaseRequisitionId.HasValue ? new List<long> { purchaseRequisitionId.Value } : new List<long>();
        return View();
    }

    [HttpPost]
    [Authorize(Policy = "PurchaseOrder_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(List<long>? purchaseRequisitionIds, long vendorId, long currencyId, int? paymentTermDays,
        DateTime orderDate, DateTime? expectedDeliveryDate, string? notes,
        List<long> itemId, List<long> warehouseId, List<decimal> quantity, List<decimal> unitPrice, List<decimal> discountPercent,
        List<string?>? sourcesJson)
    {
        if (itemId.Count != quantity.Count)
        {
            TempData["ErrorMessage"] = "Each line must have an item and quantity filled in.";
            await PopulateLookupsAsync();
            return View();
        }

        var lines = itemId.Select((id, i) => new PurchaseOrderLineRequest(
            id, warehouseId.ElementAtOrDefault(i), quantity[i], unitPrice.ElementAtOrDefault(i), discountPercent.ElementAtOrDefault(i),
            ParseSources(sourcesJson?.ElementAtOrDefault(i)))).ToList();
        var request = new CreatePurchaseOrderRequest(purchaseRequisitionIds, vendorId, currencyId, paymentTermDays, orderDate, expectedDeliveryDate, notes, lines);
        var result = await service.CreateAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to create purchase order.";
            await PopulateLookupsAsync();
            return View();
        }

        TempData["SuccessMessage"] = result.Message ?? "Purchase order created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // The Create form merges same item+warehouse lines from multiple selected requisitions client-side and
    // posts the per-requisition breakdown alongside each merged line as a JSON hidden field.
    // AllowReadingFromString guards against requisitionId/lineId being posted as JSON strings (e.g. from a select's string option values).
    private static readonly System.Text.Json.JsonSerializerOptions SourcesJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };

    private static List<PurchaseOrderLineSourceRequest>? ParseSources(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<PurchaseOrderLineSourceRequest>>(json, SourcesJsonOptions);
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    [Authorize(Policy = "PurchaseOrder_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Purchase order not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "PurchaseOrder_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, long vendorId, long currencyId, int? paymentTermDays,
        DateTime orderDate, DateTime? expectedDeliveryDate, string? notes,
        List<long> itemId, List<long> warehouseId, List<decimal> quantity, List<decimal> unitPrice, List<decimal> discountPercent)
    {
        if (itemId.Count != quantity.Count)
        {
            TempData["ErrorMessage"] = "Each line must have an item and quantity filled in.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var lines = itemId.Select((iid, i) => new PurchaseOrderLineRequest(
            iid, warehouseId.ElementAtOrDefault(i), quantity[i], unitPrice.ElementAtOrDefault(i), discountPercent.ElementAtOrDefault(i))).ToList();
        var request = new UpdatePurchaseOrderRequest(vendorId, currencyId, paymentTermDays, orderDate, expectedDeliveryDate, notes, lines);
        var result = await service.UpdateAsync(id, request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to update purchase order.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        TempData["SuccessMessage"] = result.Message ?? "Purchase order updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Purchase order not found.";
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

        // Fetch full PR detail (header + line notes) for every requisition referenced by a line source, for the PR detail popup.
        var requisitionIds = result.Data.Lines
            .SelectMany(l => l.Sources)
            .Where(s => s.PurchaseRequisitionId.HasValue)
            .Select(s => s.PurchaseRequisitionId!.Value)
            .Distinct();
        var requisitionDetails = new List<PurchaseRequisitionResponse>();
        foreach (var requisitionId in requisitionIds)
        {
            var requisitionResult = await requisitionService.GetByIdAsync(requisitionId);
            if (requisitionResult.Success && requisitionResult.Data is not null)
            {
                requisitionDetails.Add(requisitionResult.Data);
            }
        }
        ViewBag.RequisitionDetails = requisitionDetails;
        return View(result.Data);
    }

    [Authorize(Policy = "PurchaseOrder_Approve")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(long id)
    {
        var result = await service.SubmitAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [Authorize(Policy = "PurchaseOrder_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
