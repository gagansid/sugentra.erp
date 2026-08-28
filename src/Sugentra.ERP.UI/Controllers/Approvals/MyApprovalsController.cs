using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Approvals;
using Sugentra.ERP.UI.Models.Identity;
using Sugentra.ERP.UI.Services.Approvals;
using Sugentra.ERP.UI.Services.Identity;

namespace Sugentra.ERP.UI.Controllers.Approvals;

[Authorize(Policy = "ApprovalRequest_View")]
public class MyApprovalsController(ApprovalRequestApiService service, UserApiService userApiService) : Controller
{
    public async Task<IActionResult> Index(string? keyword = null, string? approverType = null, int page = 1, int pageSize = 10)
    {
        var result = await service.GetInboxAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<ApprovalInboxItem>());
        }

        var items = (result.Data ?? []).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            items = items.Where(i =>
                i.DocumentNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                i.ApproverTypeName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                i.LevelName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(approverType))
        {
            items = items.Where(i => i.DocumentType == approverType);
        }

        var itemList = items.ToList();

        ViewBag.Keyword = keyword;
        ViewBag.ApproverType = approverType;
        ViewData["ApproverTypeOptions"] = (result.Data ?? [])
            .Select(i => new { i.DocumentType, i.ApproverTypeName })
            .DistinctBy(i => i.DocumentType)
            .OrderBy(i => i.ApproverTypeName)
            .ToList();

        return View(new PagedResult<ApprovalInboxItem>
        {
            Items = itemList.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = itemList.Count
        });
    }

    public async Task<IActionResult> Details(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Approval request not found.";
            return RedirectToAction(nameof(Index));
        }

        var userIds = result.Data.Levels.SelectMany(l => l.EligibleApproverUserIds)
            .Concat(result.Data.History.Select(h => h.ApproverUserId))
            .Distinct()
            .ToList();

        var userNames = new Dictionary<long, string>();
        foreach (var userId in userIds)
        {
            var userResult = await userApiService.GetByIdAsync(userId);
            if (userResult.Success && userResult.Data is not null)
            {
                userNames[userId] = userResult.Data.FullName;
            }
        }
        ViewData["UserNames"] = userNames;

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
