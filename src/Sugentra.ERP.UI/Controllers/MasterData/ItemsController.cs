using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.MasterData;
using Sugentra.ERP.UI.Services.MasterData;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.MasterData;

[Authorize(Policy = "MasterData_View")]
public class ItemsController(ItemApiService service, UnitOfMeasurementApiService uomService) : Controller
{
    private async Task PopulateUomsAsync()
    {
        var uoms = await uomService.GetAllAsync();
        ViewBag.UnitsOfMeasurement = uoms.Data ?? [];
    }

    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, bool? isActive = null)
    {
        var result = await service.GetPagedAsync(new ItemListRequest(keyword, page, pageSize, isActive));
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<ItemListItemDto>());
        }

        ViewBag.Keyword = keyword;
        ViewBag.IsActive = isActive;
        return View(result.Data);
    }

    [Authorize(Policy = "MasterData_Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateUomsAsync();
        return View(new Item());
    }

    [HttpPost]
    [Authorize(Policy = "MasterData_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Item model)
    {
        var result = await service.CreateAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create item.");
            await PopulateUomsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Item created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "MasterData_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Item not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateUomsAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "MasterData_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Item model)
    {
        var result = await service.UpdateAsync(id, model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update item.");
            await PopulateUomsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Item updated successfully.";
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
            TempData["ErrorMessage"] = result.Message ?? "Item not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateUomsAsync();
        var adjacentResult = await service.GetAdjacentAsync(id);
        ViewBag.PreviousId = adjacentResult.Data?.PreviousId;
        ViewBag.NextId = adjacentResult.Data?.NextId;
        ViewBag.FirstId = adjacentResult.Data?.FirstId;
        ViewBag.LastId = adjacentResult.Data?.LastId;
        return View(result.Data);
    }
}
