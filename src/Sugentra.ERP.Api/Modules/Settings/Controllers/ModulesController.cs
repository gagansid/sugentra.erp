using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Queries;
using Sugentra.ERP.Api.Shared.Common;
using Module = Sugentra.ERP.Api.Modules.Settings.Entities.Module;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/modules")]
[Authorize]
public class ModulesController(CrudUseCase<Module> useCase, ModuleQuery moduleQuery) : ApiControllerBase
{
    // Powers the post-login module hub grid: any authenticated user can call this, it only ever returns
    // modules matched against their own "permission" JWT claims — no separate Module_View check needed here.
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveForCurrentUser()
    {
        var permissionCodes = User.FindAll("permission").Select(c => c.Value);
        return Success(await moduleQuery.GetActiveForPermissionCodesAsync(permissionCodes));
    }

    [HttpGet]
    [Authorize(Policy = "Module_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Module_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Module not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "Module_Create")]
    public async Task<IActionResult> Create([FromBody] Module request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/settings/modules/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Module_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] Module request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Module updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Module_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Module deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
