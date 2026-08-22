using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Modules.Settings.Queries;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/menus")]
[Authorize]
public class MenusController(CrudUseCase<Menu> useCase, MenuQuery menuQuery) : ApiControllerBase
{
    // Powers the fully dynamic sidebar: any authenticated user can call this, it only ever returns menu
    // nodes matched against their own "permission" JWT claims - no separate Menu_View check needed here.
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveForCurrentUser()
    {
        var permissionCodes = User.FindAll("permission").Select(c => c.Value);
        return Success(await menuQuery.GetActiveForPermissionCodesAsync(permissionCodes));
    }

    [HttpGet]
    [Authorize(Policy = "Menu_View")]
    public async Task<IActionResult> GetAll() => Success(await menuQuery.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Menu_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Menu not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "Menu_Create")]
    public async Task<IActionResult> Create([FromBody] Menu request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/settings/menus/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Menu_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] Menu request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Menu updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Menu_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Menu deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
