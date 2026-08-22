using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models.Approvals;
using Sugentra.ERP.UI.Models.Identity;
using Sugentra.ERP.UI.Services.Approvals;
using Sugentra.ERP.UI.Services.Identity;

namespace Sugentra.ERP.UI.Controllers.Approvals;

[Authorize(Policy = "ApprovalFlow_View")]
public class ApprovalFlowsController(ApprovalFlowApiService service, RoleApiService roleApiService, UserApiService userApiService) : Controller
{
    private async Task PopulateRolesAsync()
    {
        var roles = await roleApiService.GetPagedAsync(new RoleListRequest(null, 1, int.MaxValue));
        ViewData["AllRoles"] = roles.Data?.Items ?? [];

        var users = await userApiService.GetPagedAsync(new UserListRequest(Page: 1, PageSize: int.MaxValue));
        ViewData["AllUsers"] = users.Data?.Items ?? [];
    }

    public async Task<IActionResult> Index()
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new List<ApprovalFlowDefinitionResponse>());
        }

        return View(result.Data ?? []);
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
        string documentType, string name, decimal? minAmount, decimal? maxAmount, long? currencyId, long? warehouseId,
        int priority, bool isActive, List<int> levelNumber, List<string> levelName, List<bool> requireAllApprovers,
        List<string?> roleIds, List<string?> userIds)
    {
        var request = new SaveApprovalFlowDefinitionRequest(
            documentType, name, minAmount, maxAmount, currencyId, warehouseId, priority, isActive,
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
        long id, string documentType, string name, decimal? minAmount, decimal? maxAmount, long? currencyId, long? warehouseId,
        int priority, bool isActive, List<int> levelNumber, List<string> levelName, List<bool> requireAllApprovers,
        List<string?> roleIds, List<string?> userIds)
    {
        var request = new SaveApprovalFlowDefinitionRequest(
            documentType, name, minAmount, maxAmount, currencyId, warehouseId, priority, isActive,
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
