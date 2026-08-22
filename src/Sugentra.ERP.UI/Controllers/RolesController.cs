using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Identity;
using Sugentra.ERP.UI.Services.Identity;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers;

[Authorize(Policy = "Role_View")]
public class RolesController(RoleApiService roleApiService, ModuleApiService moduleApiService) : Controller
{
    public async Task<IActionResult> Index(string? name, int page = 1, int pageSize = 10)
    {
        ViewBag.Name = name;

        var result = await roleApiService.GetPagedAsync(new RoleListRequest(name, page, pageSize));
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<RoleListItemDto>());
        }

        return View(result.Data);
    }

    [Authorize(Policy = "Role_Create")]
    public IActionResult Create() => View(new CreateRoleRequest("", null, true));

    [HttpPost]
    [Authorize(Policy = "Role_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoleRequest request)
    {
        var result = await roleApiService.CreateAsync(request);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create role.");
            return View(request);
        }

        TempData["SuccessMessage"] = result.Message ?? "Role created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Role_Update")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await roleApiService.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "Role not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(new EditRoleViewModel
        {
            Id = result.Data.Id,
            Name = result.Data.Name,
            Description = result.Data.Description,
            IsActive = result.Data.IsActive
        });
    }

    [HttpPost]
    [Authorize(Policy = "Role_Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditRoleViewModel model)
    {
        var result = await roleApiService.UpdateAsync(model.Id, new UpdateRoleRequest(model.Name, model.Description, model.IsActive));
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update role.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Role updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Role_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await roleApiService.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message ?? (result.Success ? "Role deleted successfully." : "Failed to delete role.");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Permission_Assign")]
    public async Task<IActionResult> Permissions(long id, string name, int page = 1, int pageSize = 10,
        string? module = null, string? code = null, string? description = null, bool? isAssigned = null)
    {
        var modulesResult = await moduleApiService.GetAllAsync();
        ViewBag.Modules = modulesResult.Success
            ? modulesResult.Data!.OrderBy(m => m.SortOrder).ThenBy(m => m.Name).ToList()
            : new List<Sugentra.ERP.UI.Models.Settings.Module>();

        ViewBag.RoleId = id;
        ViewBag.RoleName = name;
        ViewBag.Module = module;
        ViewBag.Code = code;
        ViewBag.Description = description;
        ViewBag.IsAssigned = isAssigned;

        var request = new RolePermissionListRequest(module, code, description, isAssigned, page, pageSize);
        var result = await roleApiService.GetPermissionsAsync(id, request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to load role permissions.";
            return View(new PagedResult<RolePermissionListItemDto>());
        }

        return View(result.Data);
    }

    [Authorize(Policy = "Permission_Assign")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignPermission(long id, long permissionId, string name)
    {
        var result = await roleApiService.AssignPermissionAsync(id, permissionId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message ?? (result.Success ? "Permission assigned successfully." : "Failed to assign permission.");
        return RedirectToAction(nameof(Permissions), new { id, name });
    }

    [Authorize(Policy = "Permission_Assign")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokePermission(long id, long permissionId, string name)
    {
        var result = await roleApiService.RevokePermissionAsync(id, permissionId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message ?? (result.Success ? "Permission revoked successfully." : "Failed to revoke permission.");
        return RedirectToAction(nameof(Permissions), new { id, name });
    }

    [Authorize(Policy = "Permission_Assign")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePermissions(long id, string name, string? assignIds, string? revokeIds,
        int page = 1, int pageSize = 10, string? module = null, string? code = null, string? description = null, bool? isAssigned = null)
    {
        var errors = new List<string>();

        foreach (var permissionId in ParseIds(assignIds))
        {
            var result = await roleApiService.AssignPermissionAsync(id, permissionId);
            if (!result.Success)
            {
                errors.Add(result.Message ?? "Failed to assign a permission.");
            }
        }

        foreach (var permissionId in ParseIds(revokeIds))
        {
            var result = await roleApiService.RevokePermissionAsync(id, permissionId);
            if (!result.Success)
            {
                errors.Add(result.Message ?? "Failed to revoke a permission.");
            }
        }

        TempData[errors.Count == 0 ? "SuccessMessage" : "ErrorMessage"] = errors.Count == 0
            ? "Permissions updated successfully."
            : string.Join(" ", errors);

        return RedirectToAction(nameof(Permissions), new { id, name, page, pageSize, module, code, description, isAssigned });
    }

    private static IEnumerable<long> ParseIds(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? []
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(long.Parse);
}
