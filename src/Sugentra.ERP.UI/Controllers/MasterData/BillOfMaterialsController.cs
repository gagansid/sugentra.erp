using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.MasterData;
using Sugentra.ERP.UI.Services.MasterData;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.MasterData;

[Authorize(Policy = "MasterData_View")]
public class BillOfMaterialsController(BillOfMaterialApiService service, ItemApiService itemService, UnitOfMeasurementApiService uomService) : Controller
{
    private async Task PopulateLookupsAsync()
    {
        var items = await itemService.GetAllAsync();
        var uoms = await uomService.GetAllAsync();
        ViewBag.Items = items.Data ?? [];
        ViewBag.UnitsOfMeasurement = uoms.Data ?? [];
    }

    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, bool? isActive = null)
    {
        var result = await service.GetPagedAsync(new BillOfMaterialListRequest(keyword, page, pageSize, isActive));
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<BillOfMaterialListItemDto>());
        }

        ViewBag.Keyword = keyword;
        ViewBag.IsActive = isActive;
        return View(result.Data);
    }

    [Authorize(Policy = "MasterData_Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Policy = "MasterData_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(long itemId, string name, string? description, List<long> componentItemId, List<decimal> quantity, List<long> unitOfMeasurementId, List<string?> notes)
    {
        if (componentItemId.Count != quantity.Count || componentItemId.Count != unitOfMeasurementId.Count)
        {
            TempData["ErrorMessage"] = "Each component line must have a quantity and unit of measurement filled in.";
            await PopulateLookupsAsync();
            return View();
        }

        var lines = componentItemId.Select((id, i) => new BillOfMaterialLineRequest(id, quantity[i], unitOfMeasurementId[i], notes.ElementAtOrDefault(i))).ToList();
        var request = new CreateBillOfMaterialRequest(itemId, name, description, lines);
        var result = await service.CreateAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to create bill of materials.";
            await PopulateLookupsAsync();
            return View();
        }

        TempData["SuccessMessage"] = result.Message ?? "Bill of materials created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "MasterData_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Bill of materials not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "MasterData_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, string name, string? description, bool isActive, List<long> componentItemId, List<decimal> quantity, List<long> unitOfMeasurementId, List<string?> notes)
    {
        if (componentItemId.Count != quantity.Count || componentItemId.Count != unitOfMeasurementId.Count)
        {
            TempData["ErrorMessage"] = "Each component line must have a quantity and unit of measurement filled in.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var lines = componentItemId.Select((cid, i) => new BillOfMaterialLineRequest(cid, quantity[i], unitOfMeasurementId[i], notes.ElementAtOrDefault(i))).ToList();
        var request = new UpdateBillOfMaterialRequest(name, description, isActive, lines);
        var result = await service.UpdateAsync(id, request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to update bill of materials.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        TempData["SuccessMessage"] = result.Message ?? "Bill of materials updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "MasterData_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Bill of materials not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateLookupsAsync();
        var adjacentResult = await service.GetAdjacentAsync(id);
        ViewBag.PreviousId = adjacentResult.Data?.PreviousId;
        ViewBag.NextId = adjacentResult.Data?.NextId;
        ViewBag.FirstId = adjacentResult.Data?.FirstId;
        ViewBag.LastId = adjacentResult.Data?.LastId;
        return View(result.Data);
    }
}
