using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Auth;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Identity;
using Sugentra.ERP.UI.Models.Settings;
using Sugentra.ERP.UI.Services.Identity;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers;

[Authorize(Policy = "User_View")]
public class UsersController(UserApiService userApiService, AuditLogApiService auditLogApiService, RoleApiService roleApiService, PermissionApiService permissionApiService, ModuleApiService moduleApiService) : Controller
{
    private static readonly string[] ImportTemplateHeader = ["Username", "Email", "Password", "FullName", "PhoneNumber", "EmployeeId"];

    private static string CsvEscape(string? value)
    {
        value ??= string.Empty;
        return value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }

    public async Task<IActionResult> Index(string? username, string? fullName, string? email, string? status, int page = 1, int pageSize = 10)
    {
        ViewBag.Username = username;
        ViewBag.FullName = fullName;
        ViewBag.Email = email;
        ViewBag.Status = status;

        var request = new UserListRequest(username, email, fullName, status, page, pageSize);
        var result = await userApiService.GetPagedAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<UserListItemDto>());
        }

        return View(result.Data);
    }

    [Authorize(Policy = "User_Create")]
    public IActionResult Create() => View(new CreateUserViewModel());

    [HttpPost]
    [Authorize(Policy = "User_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);

        if (!ModelState.IsValid)
        {
            var validationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return isAjax
                ? Json(new ApiResponse<object?> { Success = false, Message = "Validation failed.", Errors = validationErrors })
                : View(model);
        }

        var request = new CreateUserRequest(model.Username, model.Email, model.Password, model.FullName, model.PhoneNumber, model.EmployeeId);
        var result = await userApiService.CreateAsync(request);
        if (!result.Success)
        {
            var message = result.Message ?? "Failed to create user.";
            if (isAjax)
            {
                return Json(new ApiResponse<object?> { Success = false, Message = message, Errors = new[] { message } });
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        if (isAjax)
        {
            return Json(new ApiResponse<object?> { Success = true, Message = result.Message ?? "User created successfully." });
        }

        TempData["SuccessMessage"] = result.Message ?? "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "User_Update")]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await userApiService.GetByIdAsync(id);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message ?? "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var user = result.Data;
        return View(new EditUserViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber,
            EmployeeId = user.EmployeeId
        });
    }

    [HttpPost]
    [Authorize(Policy = "User_Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return isAjax
                ? Json(new ApiResponse<object?> { Success = false, Message = "Validation failed.", Errors = errors })
                : await ReopenEditModalAsync(model);
        }

        var request = new UpdateUserRequest(model.Email, model.FullName, model.IsActive, model.PhoneNumber, model.EmployeeId);
        var result = await userApiService.UpdateAsync(model.Id, request);
        if (!result.Success)
        {
            var message = result.Message ?? "Failed to update user.";
            if (isAjax)
            {
                return Json(new ApiResponse<object?> { Success = false, Message = message, Errors = new[] { message } });
            }

            ModelState.AddModelError(string.Empty, message);
            return await ReopenEditModalAsync(model);
        }

        if (isAjax)
        {
            return Json(new ApiResponse<object?> { Success = true, Message = result.Message ?? "User updated successfully." });
        }

        TempData["SuccessMessage"] = result.Message ?? "User updated successfully.";
        return RedirectToAction(nameof(Detail), new { id = model.Id });
    }

    // Edit is only ever invoked from the modal on the Detail page now, so validation failures must
    // re-render Detail (with the modal re-opened) instead of the standalone Edit page.
    private async Task<IActionResult> ReopenEditModalAsync(EditUserViewModel model)
    {
        var detailViewModel = await BuildDetailViewModelAsync(model.Id);
        if (detailViewModel is null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["EditModel"] = model;
        ViewData["OpenEditModal"] = true;
        return View("Detail", detailViewModel);
    }

    [HttpPost]
    [Authorize(Policy = "User_ChangePassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return isAjax
                ? Json(new ApiResponse<object?> { Success = false, Message = "Validation failed.", Errors = errors })
                : await ReopenChangePasswordModalAsync(model);
        }

        var request = new ChangePasswordRequest(model.NewPassword);
        var result = await userApiService.ChangePasswordAsync(model.Id, request);
        if (!result.Success)
        {
            var message = result.Message ?? "Failed to change password.";
            if (isAjax)
            {
                return Json(new ApiResponse<object?> { Success = false, Message = message, Errors = new[] { message } });
            }

            ModelState.AddModelError(string.Empty, message);
            return await ReopenChangePasswordModalAsync(model);
        }

        if (isAjax)
        {
            // Set even for the AJAX path since the client reloads the page on success (TempData survives that next GET).
            TempData["SuccessMessage"] = result.Message ?? "Password changed successfully.";
            return Json(new ApiResponse<object?> { Success = true, Message = result.Message ?? "Password changed successfully." });
        }

        TempData["SuccessMessage"] = result.Message ?? "Password changed successfully.";
        return RedirectToAction(nameof(Detail), new { id = model.Id });
    }

    private async Task<IActionResult> ReopenChangePasswordModalAsync(ChangePasswordViewModel model)
    {
        var detailViewModel = await BuildDetailViewModelAsync(model.Id);
        if (detailViewModel is null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["ChangePasswordModel"] = model;
        ViewData["OpenChangePasswordModal"] = true;
        return View("Detail", detailViewModel);
    }

    [Authorize(Policy = "User_Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(long id)
    {
        var result = await userApiService.DeactivateAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "User_Unlock")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(long id)
    {
        var result = await userApiService.UnlockAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(long id, int permPage = 1, int permPageSize = 10, string? permModule = null, string? permCode = null, int sessionPage = 1, int sessionPageSize = 10,
        int auditPage = 1, int auditPageSize = 10, string? auditModule = null, string? auditMenu = null, string? auditAction = null, DateTime? auditFromDate = null, DateTime? auditToDate = null, string? tab = null)
    {
        // Default the date range to "today" on first load (no query string at all) so the tab doesn't open
        // showing the entire history; once the user has explicitly filtered (even to clear the dates), respect it.
        if (auditFromDate is null && auditToDate is null && !Request.Query.ContainsKey("auditFromDate") && !Request.Query.ContainsKey("auditToDate"))
        {
            auditFromDate = DateTime.Today;
            auditToDate = DateTime.Today;
        }

        var detailViewModel = await BuildDetailViewModelAsync(id, permPage, permPageSize, permModule, permCode, sessionPage, sessionPageSize, auditPage, auditPageSize, auditModule, auditMenu, auditAction, auditFromDate, auditToDate);
        if (detailViewModel is null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var filterOptionsResult = await auditLogApiService.GetFilterOptionsAsync();
        ViewBag.ActiveTab = tab;
        ViewBag.AuditModules = filterOptionsResult.Data?.Modules ?? [];
        ViewBag.AuditMenus = filterOptionsResult.Data?.Menus ?? [];
        ViewBag.AuditModule = auditModule;
        ViewBag.AuditMenu = auditMenu;
        ViewBag.AuditAction = auditAction;
        ViewBag.AuditFromDate = auditFromDate?.ToString("yyyy-MM-dd");
        ViewBag.AuditToDate = auditToDate?.ToString("yyyy-MM-dd");
        return View(detailViewModel);
    }

    private async Task<UserDetailViewModel?> BuildDetailViewModelAsync(long id, int permPage = 1, int permPageSize = 10, string? permModule = null, string? permCode = null, int sessionPage = 1, int sessionPageSize = 10,
        int auditPage = 1, int auditPageSize = 10, string? auditModule = null, string? auditMenu = null, string? auditAction = null, DateTime? auditFromDate = null, DateTime? auditToDate = null)
    {
        var userResult = await userApiService.GetByIdAsync(id);
        if (!userResult.Success || userResult.Data is null)
        {
            return null;
        }

        var rolesPermissionsResult = await userApiService.GetRolesAndPermissionsAsync(id);
        var sessionsResult = await userApiService.GetSessionsAsync(id, sessionPage, sessionPageSize);
        var auditLogsResult = await auditLogApiService.GetPagedAsync(new AuditLogListRequest(TableName: "Identity_Users", RecordId: id, ChangedByUserId: id, Action: auditAction, Module: auditModule, Menu: auditMenu, FromDate: auditFromDate, ToDate: auditToDate, Page: auditPage, PageSize: auditPageSize));
        var allRolesResult = await roleApiService.GetPagedAsync(new RoleListRequest(null, 1, int.MaxValue));
        var allPermissionsResult = await permissionApiService.GetPagedAsync(new PermissionListRequest(Page: 1, PageSize: 1000));
        var modulesResult = await moduleApiService.GetAllAsync();
        var permissionModules = modulesResult.Success
            ? modulesResult.Data!.OrderBy(m => m.SortOrder).ThenBy(m => m.Name).ToList()
            : new List<Sugentra.ERP.UI.Models.Settings.Module>();
        var permissionCatalogResult = await permissionApiService.GetPagedAsync(new PermissionListRequest(Code: permCode, Module: permModule, Page: permPage, PageSize: permPageSize));

        return new UserDetailViewModel
        {
            User = userResult.Data,
            RolesPermissions = rolesPermissionsResult.Data ?? new UserRolesPermissionsResponse([], [], []),
            Sessions = sessionsResult.Data ?? new PagedResult<UserSessionDto>(),
            AuditLogs = auditLogsResult.Data ?? new PagedResult<AuditLogListItemDto>(),
            AllRoles = allRolesResult.Data?.Items ?? [],
            PermissionCatalog = permissionCatalogResult.Data ?? new PagedResult<PermissionListItemDto>(),
            PermissionModules = permissionModules,
            PermModule = permModule,
            PermCode = permCode
        };
    }

    [Authorize(Policy = "User_Update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRoles(long id, long[] roleIds, string? originalRoleIds)
    {
        if (roleIds.Length == 0)
        {
            TempData["ErrorMessage"] = "A user must have at least one role.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var original = (originalRoleIds ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(long.Parse)
            .ToHashSet();
        var selected = roleIds.ToHashSet();

        var failures = new List<string>();
        foreach (var roleId in selected.Except(original))
        {
            var result = await userApiService.AssignRoleAsync(id, roleId);
            if (!result.Success && result.Message is not null)
            {
                failures.Add(result.Message);
            }
        }

        foreach (var roleId in original.Except(selected))
        {
            var result = await userApiService.RevokeRoleAsync(id, roleId);
            if (!result.Success && result.Message is not null)
            {
                failures.Add(result.Message);
            }
        }

        TempData[failures.Count == 0 ? "SuccessMessage" : "ErrorMessage"] =
            failures.Count == 0 ? "Roles updated successfully." : string.Join(" ", failures);
        return RedirectToAction(nameof(Detail), new { id });
    }

    [Authorize(Policy = "Permission_Assign")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePermissions(long id, string? allowIds, string? denyIds, string? removeIds, int permPage = 1, int permPageSize = 10, string? permModule = null, string? permCode = null)
    {
        static long[] ParseIds(string? csv) =>
            (csv ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();

        var failures = new List<string>();

        foreach (var permissionId in ParseIds(allowIds))
        {
            var result = await userApiService.SetPermissionOverrideAsync(id, permissionId, true);
            if (!result.Success && result.Message is not null)
            {
                failures.Add(result.Message);
            }
        }

        foreach (var permissionId in ParseIds(denyIds))
        {
            var result = await userApiService.SetPermissionOverrideAsync(id, permissionId, false);
            if (!result.Success && result.Message is not null)
            {
                failures.Add(result.Message);
            }
        }

        foreach (var permissionId in ParseIds(removeIds))
        {
            var result = await userApiService.RemovePermissionOverrideAsync(id, permissionId);
            if (!result.Success && result.Message is not null)
            {
                failures.Add(result.Message);
            }
        }

        TempData[failures.Count == 0 ? "SuccessMessage" : "ErrorMessage"] =
            failures.Count == 0 ? "Permissions updated successfully." : string.Join(" ", failures);
        return RedirectToAction(nameof(Detail), new { id, permPage, permPageSize, permModule, permCode });
    }

    [Authorize(Policy = "User_Update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeSession(long id, long sessionId)
    {
        var result = await userApiService.RevokeSessionAsync(id, sessionId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    public async Task<IActionResult> Export(string? username, string? fullName, string? email, string? status)
    {
        var request = new UserListRequest(username, email, fullName, status, 1, int.MaxValue);
        var result = await userApiService.GetPagedAsync(request);
        var items = result.Data?.Items ?? [];

        var csv = new StringBuilder();
        csv.AppendLine("Username,FullName,Email,Status");
        foreach (var user in items)
        {
            var userStatus = user switch
            {
                { IsBanned: true } => "Banned",
                { LockoutEnd: not null } when user.LockoutEnd > DateTime.Now => "Locked",
                { IsActive: false } => "Inactive",
                _ => "Active"
            };
            csv.AppendLine(string.Join(',', CsvEscape(user.Username), CsvEscape(user.FullName), CsvEscape(user.Email), CsvEscape(userStatus)));
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"users-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
    }

    [Authorize(Policy = "User_Create")]
    public IActionResult ImportTemplate()
    {
        var csv = string.Join(',', ImportTemplateHeader) + Environment.NewLine;
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "users-import-template.csv");
    }

    [HttpPost]
    [Authorize(Policy = "User_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please choose a CSV file to import.";
            return RedirectToAction(nameof(Index));
        }

        using var reader = new StreamReader(file.OpenReadStream());
        var header = await reader.ReadLineAsync();
        var lineNumber = 1;
        var successCount = 0;
        var errors = new List<string>();

        while (await reader.ReadLineAsync() is { } line)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = line.Split(',');
            if (columns.Length < 4)
            {
                errors.Add($"Row {lineNumber}: expected columns {string.Join('/', ImportTemplateHeader)}.");
                continue;
            }

            var request = new CreateUserRequest(
                columns[0].Trim(),
                columns[1].Trim(),
                columns[2].Trim(),
                columns[3].Trim(),
                columns.Length > 4 ? columns[4].Trim() : null,
                columns.Length > 5 ? columns[5].Trim() : null);

            var result = await userApiService.CreateAsync(request);
            if (result.Success)
            {
                successCount++;
            }
            else
            {
                errors.Add($"Row {lineNumber} ({request.Username}): {result.Message}");
            }
        }

        TempData["SuccessMessage"] = $"Imported {successCount} user(s).";
        if (errors.Count > 0)
        {
            TempData["ErrorMessage"] = string.Join(" | ", errors.Take(5)) + (errors.Count > 5 ? $" (+{errors.Count - 5} more)" : "");
        }

        return RedirectToAction(nameof(Index));
    }
}
