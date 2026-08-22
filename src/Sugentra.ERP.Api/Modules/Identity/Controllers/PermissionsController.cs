using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.UseCases;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Identity.Controllers;

[Route("api/identity/permissions")]
[Authorize]
public class PermissionsController(PermissionUseCase permissionUseCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission_View")]
    public async Task<IActionResult> GetPaged([FromQuery] PermissionListRequest request)
    {
        var result = await permissionUseCase.GetPagedAsync(request);
        return Success(result);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Permission_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await permissionUseCase.GetByIdAsync(id);
        return result is not null ? Success(result) : Failure("Permission not found.", StatusCodes.Status404NotFound);
    }

    [HttpPost]
    [Authorize(Policy = "Permission_Assign")]
    public async Task<IActionResult> Create([FromBody] CreatePermissionRequest request)
    {
        var result = await permissionUseCase.CreateAsync(request);
        return result.IsSuccess
            ? SuccessCreated($"api/identity/permissions/{result.Value!.Id}", result.Value)
            : Failure(result.Error!, StatusCodes.Status409Conflict);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Permission_Assign")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePermissionRequest request)
    {
        var result = await permissionUseCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Permission updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Permission_Assign")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await permissionUseCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Permission deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
