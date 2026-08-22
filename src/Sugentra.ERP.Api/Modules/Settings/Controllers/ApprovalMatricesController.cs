using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/approval-matrices")]
[Authorize]
public class ApprovalMatricesController(CrudUseCase<ApprovalMatrix> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "ApprovalMatrix_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "ApprovalMatrix_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Approval matrix entry not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "ApprovalMatrix_Create")]
    public async Task<IActionResult> Create([FromBody] ApprovalMatrix request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/settings/approval-matrices/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "ApprovalMatrix_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] ApprovalMatrix request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Approval matrix entry updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "ApprovalMatrix_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Approval matrix entry deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
