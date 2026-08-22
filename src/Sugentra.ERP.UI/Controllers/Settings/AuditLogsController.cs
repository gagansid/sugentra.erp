using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Settings;

[Authorize(Policy = "AuditLog_View")]
public class AuditLogsController(AuditLogApiService service) : Controller
{
    public async Task<IActionResult> Index(string? module, string? menu, string? actionType, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
    {
        // Default the date range to "today" on first load (no query string at all) so the page doesn't open
        // showing the entire history; once the user has explicitly filtered (even to clear the dates), respect it.
        if (fromDate is null && toDate is null && !Request.Query.ContainsKey("fromDate") && !Request.Query.ContainsKey("toDate"))
        {
            fromDate = DateTime.Today;
            toDate = DateTime.Today;
        }

        var request = new AuditLogListRequest(Action: actionType, Module: module, Menu: menu, FromDate: fromDate, ToDate: toDate, Page: page, PageSize: pageSize);
        var result = await service.GetPagedAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<AuditLogListItemDto>());
        }

        var filterOptionsResult = await service.GetFilterOptionsAsync();
        ViewBag.Modules = filterOptionsResult.Data?.Modules ?? [];
        ViewBag.Menus = filterOptionsResult.Data?.Menus ?? [];
        ViewBag.Module = module;
        ViewBag.Menu = menu;
        ViewBag.Action = actionType;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        return View(result.Data);
    }

    public async Task<IActionResult> Detail(long id, string? source = null, long? userId = null)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Audit log entry not found.";
            return RedirectToAction(nameof(Index));
        }

        var isUserSource = string.Equals(source, "user", StringComparison.OrdinalIgnoreCase);
        // The entry being viewed might belong to a different table (e.g. something this user changed
        // elsewhere), so the scoping user id is whatever the Users tab passed in, not this entry's own RecordId.
        var scopeUserId = userId ?? result.Data.RecordId;
        if (isUserSource)
        {
            // Keep the sidebar/workspace anchored on Identity (where the Users tab lives) regardless of which
            // module the viewed entry's own table belongs to, and highlight the Users menu item (this page has
            // no menu entry of its own).
            ViewData["ModuleCode"] = "Identity";
            ViewData["ActiveController"] = "Users";
        }

        var adjacentResult = isUserSource
            ? await service.GetAdjacentAsync(id, "Identity_Users", scopeUserId, scopeUserId)
            : await service.GetAdjacentAsync(id);
        ViewBag.PreviousId = adjacentResult.Data?.PreviousId;
        ViewBag.NextId = adjacentResult.Data?.NextId;
        ViewBag.FirstId = adjacentResult.Data?.FirstId;
        ViewBag.LastId = adjacentResult.Data?.LastId;
        ViewBag.Source = source;
        ViewBag.UserId = isUserSource ? scopeUserId : (long?)null;
        return View(result.Data);
    }
}

