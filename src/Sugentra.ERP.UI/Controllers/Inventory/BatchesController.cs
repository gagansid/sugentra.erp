using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Inventory;
using Sugentra.ERP.UI.Services.Inventory;
using Sugentra.ERP.UI.Services.MasterData;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Inventory;

[Authorize(Policy = "Batch_View")]
public class BatchesController(BatchApiService service, ItemApiService itemService, WarehouseApiService warehouseService) : Controller
{
    private async Task PopulateLookupsAsync()
    {
        ViewBag.Items = (await itemService.GetAllAsync()).Data ?? [];
        ViewBag.Warehouses = (await warehouseService.GetAllAsync()).Data ?? [];
    }

    /// <summary>Maps the API's field-&gt;messages error dictionary onto ModelState so views can show per-field messages.</summary>
    private void ApplyValidationErrors(object? errors, string? fallbackMessage)
    {
        var applied = false;
        if (errors is System.Text.Json.JsonElement { ValueKind: System.Text.Json.JsonValueKind.Object } element)
        {
            foreach (var field in element.EnumerateObject())
            {
                if (field.Value.ValueKind != System.Text.Json.JsonValueKind.Array) continue;
                foreach (var message in field.Value.EnumerateArray())
                {
                    ModelState.AddModelError(field.Name, message.GetString() ?? "Invalid value.");
                    applied = true;
                }
            }
        }

        if (!applied)
        {
            ModelState.AddModelError(string.Empty, fallbackMessage ?? "Failed to save batch.");
        }
    }

    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, long? warehouseId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<Batch>());
        }

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            all = all.Where(b =>
                b.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (b.LegalityDocumentNumber?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (b.SourceReference?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
        if (warehouseId.HasValue)
        {
            all = all.Where(b => b.WarehouseId == warehouseId.Value).ToList();
        }
        if (dateFrom.HasValue)
        {
            all = all.Where(b => b.ReceivedDate.Date >= dateFrom.Value.Date).ToList();
        }
        if (dateTo.HasValue)
        {
            all = all.Where(b => b.ReceivedDate.Date <= dateTo.Value.Date).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.WarehouseId = warehouseId;
        ViewBag.DateFrom = dateFrom;
        ViewBag.DateTo = dateTo;
        await PopulateLookupsAsync();
        return View(new PagedResult<Batch>
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
        return View(new Batch());
    }

    [HttpPost]
    [Authorize(Policy = "Batch_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Batch model)
    {
        var result = await service.CreateAsync(model);
        if (!result.Success)
        {
            ApplyValidationErrors(result.Errors, result.Message ?? "Failed to create batch.");
            await PopulateLookupsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Batch created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Batch_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Batch not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        ViewBag.HasStock = (await service.HasStockAsync(id)).Data;
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "Batch_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Batch model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ApplyValidationErrors(result.Errors, result.Message ?? "Failed to update batch.");
            await PopulateLookupsAsync();
            ViewBag.HasStock = (await service.HasStockAsync(id)).Data;
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Batch updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Batch not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        return View(result.Data);
    }

    [Authorize(Policy = "Batch_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message ?? "Batch deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? "Batch cannot be deleted because it still has stock or related records.";
        }

        return RedirectToAction(nameof(Index));
    }
}
