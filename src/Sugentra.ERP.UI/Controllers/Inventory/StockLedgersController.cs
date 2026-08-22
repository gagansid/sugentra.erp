using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Inventory;
using Sugentra.ERP.UI.Services.Inventory;
using Sugentra.ERP.UI.Services.MasterData;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Inventory;

/// <summary>Read-only report: the stock ledger (kartu stok) is an immutable append-only audit trail.</summary>
[Authorize(Policy = "StockLedger_View")]
public class StockLedgersController(StockLedgerApiService service, ItemApiService itemService, WarehouseApiService warehouseService) : Controller
{
    public async Task<IActionResult> Index(string? keyword = null, int page = 1, int pageSize = 10, string? movementType = null, long? warehouseId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<StockLedger>());
        }

        var items = (await itemService.GetAllAsync()).Data ?? [];
        ViewBag.Items = items;
        ViewBag.Warehouses = (await warehouseService.GetAllAsync()).Data ?? [];

        var all = result.Data ?? [];
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var itemNames = items.Where(i => i.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)).Select(i => i.Id).ToHashSet();
            all = all.Where(l => itemNames.Contains(l.ItemId) ||
                (l.ReferenceType?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
        if (!string.IsNullOrWhiteSpace(movementType))
        {
            all = all.Where(l => l.MovementType == movementType).ToList();
        }
        if (warehouseId.HasValue)
        {
            all = all.Where(l => l.WarehouseId == warehouseId.Value).ToList();
        }
        if (dateFrom.HasValue)
        {
            all = all.Where(l => l.MovementDate.Date >= dateFrom.Value.Date).ToList();
        }
        if (dateTo.HasValue)
        {
            all = all.Where(l => l.MovementDate.Date <= dateTo.Value.Date).ToList();
        }

        ViewBag.Keyword = keyword;
        ViewBag.MovementType = movementType;
        ViewBag.WarehouseId = warehouseId;
        ViewBag.DateFrom = dateFrom;
        ViewBag.DateTo = dateTo;
        return View(new PagedResult<StockLedger>
        {
            Items = all.OrderByDescending(l => l.MovementDate).Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }
}
