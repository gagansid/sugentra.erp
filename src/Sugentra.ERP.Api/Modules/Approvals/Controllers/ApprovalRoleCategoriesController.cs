using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Approvals.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Approvals.Controllers;

[Route("api/approvals/role-categories")]
[Authorize]
public class ApprovalRoleCategoriesController(CrudUseCase<ApprovalRoleCategory> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "ApprovalRoleCategory_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "ApprovalRoleCategory_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Approval role category not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "ApprovalRoleCategory_Create")]
    public async Task<IActionResult> Create([FromBody] ApprovalRoleCategory request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/approvals/role-categories/{created.Id}", created);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "ApprovalRoleCategory_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Approval role category deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
