using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Approvals;
using Sugentra.ERP.UI.Models.Identity;
using Sugentra.ERP.UI.Services.Approvals;
using Sugentra.ERP.UI.Services.Identity;

namespace Sugentra.ERP.UI.Controllers.Approvals;

[Authorize(Policy = "ApprovalFlow_View")]
public class ApprovalFlowsController(ApprovalFlowApiService service, RoleApiService roleApiService, UserApiService userApiService, ApprovalRoleCategoryApiService roleCategoryService) : Controller
{
    private async Task PopulateRolesAsync()
    {
        var roles = await roleApiService.GetPagedAsync(new RoleListRequest(null, 1, int.MaxValue));
        var allRoles = roles.Data?.Items ?? [];

        var users = await userApiService.GetPagedAsync(new UserListRequest(Page: 1, PageSize: int.MaxValue));
        ViewData["AllUsers"] = users.Data?.Items ?? [];

        var roleCategories = (await roleCategoryService.GetAllAsync()).Data ?? [];

        // Only roles configured as an Approver Role (Settings > Approval Types) are eligible to be picked in a flow level.
        var configuredRoleIds = roleCategories.Select(rc => rc.RoleId).ToHashSet();
        ViewData["AllRoles"] = allRoles.Where(r => configuredRoleIds.Contains(r.Id)).ToList();

        ViewData["ApproverTypes"] = roleCategories
            .GroupBy(rc => rc.ApproverType)
            .Select(g => new ApproverTypeOption(g.Key, g.First().Description))
            .OrderBy(x => x.ApproverType)
            .ToList();
    }

    public async Task<IActionResult> Index(string? keyword = null, bool? isActive = null, int page = 1, int pageSize = 10)
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<ApprovalFlowDefinitionResponse>());
        }

        var flows = (result.Data ?? []).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            flows = flows.Where(f =>
                f.ApproverType.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                f.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        if (isActive.HasValue)
        {
            flows = flows.Where(f => f.IsActive == isActive.Value);
        }

        var flowList = flows.ToList();

        ViewBag.Keyword = keyword;
        ViewBag.IsActive = isActive;

        return View(new PagedResult<ApprovalFlowDefinitionResponse>
        {
            Items = flowList.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = flowList.Count
        });
    }

    [Authorize(Policy = "ApprovalFlow_Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateRolesAsync();
        return View(new ApprovalFlowDefinitionResponse(0, string.Empty, string.Empty, null, null, null, null, 0, true, []));
    }

    [HttpPost]
    [Authorize(Policy = "ApprovalFlow_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string approverType, string name, decimal? minAmount, decimal? maxAmount, long? currencyId, long? warehouseId,
        int priority, bool isActive, List<int> levelNumber, List<string> levelName, List<bool> requireAllApprovers,
        List<string?> roleIds, List<string?> userIds)
    {
        var request = new SaveApprovalFlowDefinitionRequest(
            approverType, name, minAmount, maxAmount, currencyId, warehouseId, priority, isActive,
            BuildLevels(levelNumber, levelName, requireAllApprovers, roleIds, userIds));

        var result = await service.CreateAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to create approval flow.";
            return RedirectToAction(nameof(Create));
        }

        TempData["SuccessMessage"] = result.Message ?? "Approval flow created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "ApprovalFlow_Edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Approval flow not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateRolesAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Policy = "ApprovalFlow_Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        long id, string approverType, string name, decimal? minAmount, decimal? maxAmount, long? currencyId, long? warehouseId,
        int priority, bool isActive, List<int> levelNumber, List<string> levelName, List<bool> requireAllApprovers,
        List<string?> roleIds, List<string?> userIds)
    {
        var request = new SaveApprovalFlowDefinitionRequest(
            approverType, name, minAmount, maxAmount, currencyId, warehouseId, priority, isActive,
            BuildLevels(levelNumber, levelName, requireAllApprovers, roleIds, userIds));

        var result = await service.UpdateAsync(id, request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to update approval flow.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        TempData["SuccessMessage"] = result.Message ?? "Approval flow updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = "ApprovalFlow_Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message ?? (result.Success ? "Approval flow deleted successfully." : "Failed to delete approval flow.");
        return RedirectToAction(nameof(Index));
    }

    // Level rows are posted as parallel arrays; RoleIds/UserIds per level are comma-separated id lists from the form.
    private static List<ApprovalFlowLevelDto> BuildLevels(
        List<int> levelNumber, List<string> levelName, List<bool> requireAllApprovers, List<string?> roleIds, List<string?> userIds)
    {
        var levels = new List<ApprovalFlowLevelDto>();
        for (var i = 0; i < levelNumber.Count; i++)
        {
            var approvers = new List<ApprovalFlowLevelApproverDto>();
            approvers.AddRange(ParseIds(roleIds.ElementAtOrDefault(i)).Select(rid => new ApprovalFlowLevelApproverDto(rid, null)));
            approvers.AddRange(ParseIds(userIds.ElementAtOrDefault(i)).Select(uid => new ApprovalFlowLevelApproverDto(null, uid)));

            levels.Add(new ApprovalFlowLevelDto(
                levelNumber[i], levelName.ElementAtOrDefault(i) ?? $"Level {levelNumber[i]}",
                requireAllApprovers.ElementAtOrDefault(i), approvers));
        }

        return levels;
    }

    private static IEnumerable<long> ParseIds(string? raw) =>
        (raw ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => long.TryParse(s, out _)).Select(long.Parse);
}
