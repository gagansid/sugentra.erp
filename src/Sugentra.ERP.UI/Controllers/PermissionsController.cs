using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Identity;
using Sugentra.ERP.UI.Services.Identity;
using Sugentra.ERP.UI.Services.Settings;

namespace Sugentra.ERP.UI.Controllers;

[Authorize(Policy = "Permission_View")]
public class PermissionsController(PermissionApiService permissionApiService, ModuleApiService moduleApiService) : Controller
{
    public async Task<IActionResult> Index(string? code, string? module, int page = 1, int pageSize = 10)
    {
        ViewBag.Code = code;
        ViewBag.Module = module;

        var modulesResult = await moduleApiService.GetAllAsync();
        ViewBag.Modules = modulesResult.Success
            ? modulesResult.Data!.OrderBy(m => m.SortOrder).ThenBy(m => m.Name).ToList()
            : new List<Sugentra.ERP.UI.Models.Settings.Module>();

        var result = await permissionApiService.GetPagedAsync(new PermissionListRequest(Code: code, Module: module, Page: page, PageSize: pageSize));
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new PagedResult<PermissionListItemDto>());
        }

        return View(result.Data);
    }
}
