using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.Queries;
using Sugentra.ERP.Api.Modules.Identity.UseCases;

namespace Sugentra.ERP.Api.Modules.Identity.Controllers;

[ApiController]
[Route("api/identity/users")]
public class UsersController(UserUseCase userUseCase, UserListQuery userListQuery) : ControllerBase
{
    // NOTE: [Authorize] policies are added in Phase D once PermissionPolicyProvider/JWT are wired.

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await userListQuery.GetPagedAsync(page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await userUseCase.CreateAsync(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequest request)
    {
        var result = await userUseCase.UpdateAsync(id, request);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Deactivate(long id)
    {
        var result = await userUseCase.DeactivateAsync(id);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
