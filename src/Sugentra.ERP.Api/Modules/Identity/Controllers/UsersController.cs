using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.UseCases;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Identity.Controllers;

[Route("api/identity/users")]
[Authorize]
public class UsersController(UserUseCase userUseCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "User_View")]
    public async Task<IActionResult> GetPaged([FromQuery] UserListRequest request)
    {
        var result = await userUseCase.GetPagedAsync(request);
        return Success(result);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "User_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await userUseCase.GetByIdAsync(id);
        return result is not null ? Success(result) : Failure("User not found.", StatusCodes.Status404NotFound);
    }

    [HttpPost]
    [Authorize(Policy = "User_Create")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await userUseCase.CreateAsync(request);
        return result.IsSuccess
            ? SuccessCreated($"api/identity/users/{result.Value!.Id}", result.Value)
            : Failure(result.Error!, StatusCodes.Status409Conflict);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "User_Update")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequest request)
    {
        var result = await userUseCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "User updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "User_Delete")]
    public async Task<IActionResult> Deactivate(long id)
    {
        var result = await userUseCase.DeactivateAsync(id);
        return result.IsSuccess
            ? SuccessMessage("User deactivated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpPost("{id:long}/unlock")]
    [Authorize(Policy = "User_Unlock")]
    public async Task<IActionResult> Unlock(long id)
    {
        var result = await userUseCase.UnlockAsync(id);
        return result.IsSuccess
            ? SuccessMessage("User unlocked successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpPost("{id:long}/change-password")]
    [Authorize(Policy = "User_ChangePassword")]
    public async Task<IActionResult> ChangePassword(long id, [FromBody] ChangePasswordRequest request)
    {
        var result = await userUseCase.ChangePasswordAsync(id, request);
        return result.IsSuccess
            ? SuccessMessage("Password changed successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpPost("{id:long}/roles/{roleId:long}")]
    [Authorize(Policy = "User_Update")]
    public async Task<IActionResult> AssignRole(long id, long roleId)
    {
        var result = await userUseCase.AssignRoleAsync(id, roleId);
        return result.IsSuccess
            ? SuccessCreated($"api/identity/users/{id}/roles/{roleId}", result.Value)
            : Failure(result.Error!, StatusCodes.Status409Conflict);
    }

    [HttpDelete("{id:long}/roles/{roleId:long}")]
    [Authorize(Policy = "User_Update")]
    public async Task<IActionResult> RevokeRole(long id, long roleId)
    {
        var result = await userUseCase.RevokeRoleAsync(id, roleId);
        return result.IsSuccess
            ? SuccessMessage("Role revoked successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpPut("{id:long}/permissions/{permissionId:long}")]
    [Authorize(Policy = "Permission_Assign")]
    public async Task<IActionResult> SetPermissionOverride(long id, long permissionId, [FromBody] SetPermissionOverrideRequest request)
    {
        var result = await userUseCase.SetPermissionOverrideAsync(id, permissionId, request.IsAllowed);
        return result.IsSuccess
            ? Success(result.Value, "Permission override updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}/permissions/{permissionId:long}")]
    [Authorize(Policy = "Permission_Assign")]
    public async Task<IActionResult> RemovePermissionOverride(long id, long permissionId)
    {
        var result = await userUseCase.RemovePermissionOverrideAsync(id, permissionId);
        return result.IsSuccess
            ? SuccessMessage("Permission override removed successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpGet("{id:long}/roles-permissions")]
    [Authorize(Policy = "User_View")]
    public async Task<IActionResult> GetRolesAndPermissions(long id) =>
        Success(await userUseCase.GetRolesAndPermissionsAsync(id));

    [HttpGet("{id:long}/sessions")]
    [Authorize(Policy = "User_View")]
    public async Task<IActionResult> GetSessions(long id, int page = 1, int pageSize = 10) =>
        Success(await userUseCase.GetSessionsAsync(id, page, pageSize));

    [HttpPost("{id:long}/sessions/{sessionId:long}/revoke")]
    [Authorize(Policy = "User_Update")]
    public async Task<IActionResult> RevokeSession(long id, long sessionId)
    {
        var result = await userUseCase.RevokeSessionAsync(id, sessionId);
        return result.IsSuccess
            ? SuccessMessage("Session revoked successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
