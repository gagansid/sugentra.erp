using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.UseCases;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Identity.Controllers;

[Route("api/identity/roles")]
[Authorize]
public class RolesController(RoleUseCase roleUseCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Role_View")]
    public async Task<IActionResult> GetPaged([FromQuery] RoleListRequest request)
    {
        var result = await roleUseCase.GetPagedAsync(request);
        return Success(result);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Role_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await roleUseCase.GetByIdAsync(id);
        return result is not null ? Success(result) : Failure("Role not found.", StatusCodes.Status404NotFound);
    }

    [HttpPost]
    [Authorize(Policy = "Role_Create")]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var result = await roleUseCase.CreateAsync(request);
        return result.IsSuccess
            ? SuccessCreated($"api/identity/roles/{result.Value!.Id}", result.Value)
            : Failure(result.Error!, StatusCodes.Status409Conflict);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Role_Update")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateRoleRequest request)
    {
        var result = await roleUseCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Role updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Role_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await roleUseCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Role deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpPost("{id:long}/permissions/{permissionId:long}")]
    [Authorize(Policy = "Permission_Assign")]
    public async Task<IActionResult> AssignPermission(long id, long permissionId)
    {
        var result = await roleUseCase.AssignPermissionAsync(id, permissionId);
        return result.IsSuccess
            ? SuccessCreated($"api/identity/roles/{id}/permissions/{permissionId}", result.Value)
            : Failure(result.Error!, StatusCodes.Status409Conflict);
    }

    [HttpDelete("{id:long}/permissions/{permissionId:long}")]
    [Authorize(Policy = "Permission_Assign")]
    public async Task<IActionResult> RevokePermission(long id, long permissionId)
    {
        var result = await roleUseCase.RevokePermissionAsync(id, permissionId);
        return result.IsSuccess
            ? SuccessMessage("Permission revoked successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpGet("{id:long}/permissions")]
    [Authorize(Policy = "Permission_Assign")]
    public async Task<IActionResult> GetPermissions(long id, [FromQuery] RolePermissionListRequest request)
    {
        var result = await roleUseCase.GetPermissionsAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value)
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
