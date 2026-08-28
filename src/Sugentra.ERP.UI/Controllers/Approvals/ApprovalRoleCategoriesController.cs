using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Approvals;
using Sugentra.ERP.UI.Models.Identity;
using Sugentra.ERP.UI.Services.Approvals;
using Sugentra.ERP.UI.Services.Identity;

namespace Sugentra.ERP.UI.Controllers.Approvals;

[Authorize(Policy = "ApprovalRoleCategory_View")]
public class ApprovalRoleCategoriesController(ApprovalRoleCategoryApiService service, RoleApiService roleApiService) : Controller
{
    private async Task<IReadOnlyList<RoleListItemDto>> PopulateRolesAsync()
    {
        var roles = await roleApiService.GetPagedAsync(new RoleListRequest(null, 1, int.MaxValue));
        var allRoles = roles.Data?.Items ?? [];
        ViewData["AllRoles"] = allRoles;
        return allRoles;
    }

    public async Task<IActionResult> Index(string? keyword = null, long? roleId = null, int page = 1, int pageSize = 10)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<ApprovalRoleCategoryGroupViewModel>());
        }

        var allRoles = await PopulateRolesAsync();

        var groups = (result.Data ?? [])
            .GroupBy(x => x.ApproverType)
            .Select(g => new ApprovalRoleCategoryGroupViewModel(g.Key, g.ToList()))
            .OrderBy(g => g.ApproverType)
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            groups = groups.Where(g =>
                g.ApproverType.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (g.Entries.FirstOrDefault()?.Description?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                g.Entries.Any(e => (allRoles.FirstOrDefault(r => r.Id == e.RoleId)?.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)));
        }

        if (roleId.HasValue)
        {
            groups = groups.Where(g => g.Entries.Any(e => e.RoleId == roleId.Value));
        }

        var groupList = groups.ToList();

        ViewBag.Keyword = keyword;
        ViewBag.RoleId = roleId;

        return View(new PagedResult<ApprovalRoleCategoryGroupViewModel>
        {
            Items = groupList.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = groupList.Count
        });
    }

    [Authorize(Policy = "ApprovalRoleCategory_Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateRolesAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Policy = "ApprovalRoleCategory_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string approverType, long[] roleIds, string? description)
    {
        if (roleIds.Length == 0)
        {
            TempData["ErrorMessage"] = "Select at least one role.";
            return RedirectToAction(nameof(Create));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            TempData["ErrorMessage"] = "Approver name is required.";
            return RedirectToAction(nameof(Create));
        }

        string? error = null;
        foreach (var roleId in roleIds)
        {
            var result = await service.CreateAsync(new ApprovalRoleCategoryResponse(0, roleId, approverType, description));
            if (!result.Success)
            {
                error = result.Message;
            }
        }

        TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? "Approval role category created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "ApprovalRoleCategory_Edit")]
    public async Task<IActionResult> Edit(string approverType)
    {
        await PopulateRolesAsync();

        var result = await service.GetAllAsync();
        var entries = (result.Data ?? []).Where(x => x.ApproverType == approverType).ToList();
        if (entries.Count == 0)
        {
            TempData["ErrorMessage"] = "Approver type not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(new ApprovalRoleCategoryGroupViewModel(approverType, entries));
    }

    [HttpPost]
    [Authorize(Policy = "ApprovalRoleCategory_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string originalApproverType, string approverType, long[] roleIds, string? description)
    {
        if (roleIds.Length == 0)
        {
            TempData["ErrorMessage"] = "Select at least one role.";
            return RedirectToAction(nameof(Edit), new { approverType = originalApproverType });
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            TempData["ErrorMessage"] = "Approver name is required.";
            return RedirectToAction(nameof(Edit), new { approverType = originalApproverType });
        }

        var result = await service.GetAllAsync();
        var existing = (result.Data ?? []).Where(x => x.ApproverType == originalApproverType).ToList();

        string? error = null;
        foreach (var entry in existing)
        {
            var deleteResult = await service.DeleteAsync(entry.Id);
            if (!deleteResult.Success)
            {
                error = deleteResult.Message;
            }
        }

        foreach (var roleId in roleIds)
        {
            var createResult = await service.CreateAsync(new ApprovalRoleCategoryResponse(0, roleId, approverType, description));
            if (!createResult.Success)
            {
                error = createResult.Message;
            }
        }

        TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? "Approval role category updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = "ApprovalRoleCategory_Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string approverType)
    {
        var result = await service.GetAllAsync();
        var entries = (result.Data ?? []).Where(x => x.ApproverType == approverType).ToList();

        string? error = null;
        foreach (var entry in entries)
        {
            var deleteResult = await service.DeleteAsync(entry.Id);
            if (!deleteResult.Success)
            {
                error = deleteResult.Message;
            }
        }

        TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? "Approval role category deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}

