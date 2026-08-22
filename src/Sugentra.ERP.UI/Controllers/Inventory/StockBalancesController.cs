using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Inventory;
using Sugentra.ERP.UI.Services.Inventory;
using Sugentra.ERP.UI.Services.MasterData;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Inventory;

/// <summary>Read-only report: stock balances are a system-computed snapshot, never edited from the UI.</summary>
[Authorize(Policy = "StockBalance_View")]
public class StockBalancesController(StockBalanceApiService service, ItemApiService itemService, WarehouseApiService warehouseService) : Controller
{
    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, long? warehouseId = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<StockBalance>());
        }

        var items = (await itemService.GetAllAsync()).Data ?? [];
        var warehouses = (await warehouseService.GetAllAsync()).Data ?? [];
        ViewBag.Items = items;
        ViewBag.Warehouses = warehouses;

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var itemNames = items.Where(i => i.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)).Select(i => i.Id).ToHashSet();
            all = all.Where(b => itemNames.Contains(b.ItemId)).ToList();
        }
        if (warehouseId.HasValue)
        {
            all = all.Where(b => b.WarehouseId == warehouseId.Value).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.WarehouseId = warehouseId;
        return View(new PagedResult<StockBalance>
        {
            Items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }
}
