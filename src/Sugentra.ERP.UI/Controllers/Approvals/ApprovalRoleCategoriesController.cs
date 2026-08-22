using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models.Approvals;
using Sugentra.ERP.UI.Services.Approvals;

namespace Sugentra.ERP.UI.Controllers.Approvals;

[Authorize(Policy = "ApprovalRoleCategory_View")]
public class ApprovalRoleCategoriesController(ApprovalRoleCategoryApiService service) : Controller
{
    public async Task<IActionResult> Index()
    {
        var result = await service.GetAllAsync();
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new List<ApprovalRoleCategoryResponse>());
        }

        return View(result.Data ?? []);
    }

    [Authorize(Policy = "ApprovalRoleCategory_Create")]
    public IActionResult Create() => View(new ApprovalRoleCategoryResponse(0, 0, string.Empty));

    [HttpPost]
    [Authorize(Policy = "ApprovalRoleCategory_Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(long roleId, string documentType)
    {
        var result = await service.CreateAsync(new ApprovalRoleCategoryResponse(0, roleId, documentType));
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to create approval role category.";
            return RedirectToAction(nameof(Create));
        }

        TempData["SuccessMessage"] = result.Message ?? "Approval role category created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = "ApprovalRoleCategory_Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await service.DeleteAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message ?? (result.Success ? "Approval role category deleted successfully." : "Failed to delete approval role category.");
        return RedirectToAction(nameof(Index));
    }
}
