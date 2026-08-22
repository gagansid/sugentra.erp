using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers.Settings;

[Authorize(Policy = "EmailHistory_View")]
public class EmailHistoryController(EmailHistoryApiService service) : Controller
{
    public async Task<IActionResult> Index(string? toEmail, string? templateCode, string? status, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
    {
        var request = new EmailHistoryListRequest(ToEmail: toEmail, TemplateCode: templateCode, Status: status, FromDate: fromDate, ToDate: toDate, Page: page, PageSize: pageSize);
        var result = await service.GetPagedAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<EmailHistoryListItemDto>());
        }

        ViewBag.ToEmail = toEmail;
        ViewBag.TemplateCode = templateCode;
        ViewBag.Status = status;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        return View(result.Data);
    }

    public async Task<IActionResult> Detail(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Email history entry not found.";
            return RedirectToAction(nameof(Index));
        }

        var adjacentResult = await service.GetAdjacentAsync(id);
        ViewBag.PreviousId = adjacentResult.Data?.PreviousId;
        ViewBag.NextId = adjacentResult.Data?.NextId;
        ViewBag.FirstId = adjacentResult.Data?.FirstId;
        ViewBag.LastId = adjacentResult.Data?.LastId;
        return View(result.Data);
    }
}
