using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Auth;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Shared;
using Sugentra.ERP.UI.Services.Shared;

namespace Sugentra.ERP.UI.Controllers.Shared;

[Authorize(Policy = "ErrorLog_View")]
public class ErrorLogsController(ErrorLogApiService service) : Controller
{
    public async Task<IActionResult> Index(string? source, string? search, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
    {
        var request = new ErrorLogListRequest(Source: source, Search: search, FromDate: fromDate, ToDate: toDate, Page: page, PageSize: pageSize);
        var result = await service.GetPagedAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<ErrorLogListItemDto>());
        }

        ViewBag.Source = source;
        ViewBag.Search = search;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        return View(result.Data);
    }

    public async Task<IActionResult> Detail(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Error log entry not found.";
            return RedirectToAction(nameof(Index));
        }

        var adjacentResult = await service.GetAdjacentAsync(id);
        ViewBag.PreviousId = adjacentResult.Data?.PreviousId;
        ViewBag.NextId = adjacentResult.Data?.NextId;
        ViewBag.FirstId = adjacentResult.Data?.FirstId;
        ViewBag.LastId = adjacentResult.Data?.LastId;

        if (User.HasPermission("ErrorLog_SendEmail"))
        {
            var defaultEmailResult = await service.GetDefaultNotificationEmailAsync();
            ViewBag.DefaultNotificationEmail = defaultEmailResult.Data;
        }

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ErrorLog_Edit")]
    public async Task<IActionResult> UpdateStatus(long id, string status, string? remarks)
    {
        var result = await service.UpdateStatusAsync(id, status, remarks);
        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ErrorLog_SendEmail")]
    public async Task<IActionResult> SendEmail(long id, string toEmail)
    {
        var result = await service.SendEmailAsync(id, toEmail);
        return Json(new { success = result.Success, message = result.Message });
    }
}
