using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models.Approvals;
using Sugentra.ERP.UI.Services.Approvals;

namespace Sugentra.ERP.UI.Controllers.Approvals;

[Authorize(Policy = "ApprovalRequest_View")]
public class MyApprovalsController(ApprovalRequestApiService service) : Controller
{
    public async Task<IActionResult> Index()
    {
        var result = await service.GetInboxAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new List<ApprovalInboxItem>());
        }

        return View(result.Data ?? []);
    }

    public async Task<IActionResult> Details(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Approval request not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "ApprovalRequest_Act")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Act(long id, bool approve, string? comment)
    {
        var result = await service.ActAsync(id, new ApprovalActionRequest(approve, comment));
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message ?? (result.Success ? "Action recorded." : "Failed to record action.");
        return RedirectToAction(nameof(Index));
    }
}
