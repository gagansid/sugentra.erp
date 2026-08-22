using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Approvals.Dtos;
using Sugentra.ERP.Api.Modules.Approvals.UseCases;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Approvals.Controllers;

// Manages reusable approval flow configurations ("jenjang" definitions) — the admin menu for this module.
[Route("api/approvals/flows")]
[Authorize]
public class ApprovalFlowsController(ApprovalFlowUseCase useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "ApprovalFlow_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "ApprovalFlow_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var response = await useCase.GetByIdAsync(id);
        return response is null ? Failure("Approval flow not found.", StatusCodes.Status404NotFound) : Success(response);
    }

    [HttpPost]
    [Authorize(Policy = "ApprovalFlow_Create")]
    public async Task<IActionResult> Create([FromBody] SaveApprovalFlowDefinitionRequest request)
    {
        var result = await useCase.CreateAsync(request);
        return result.IsSuccess
            ? SuccessCreated($"api/approvals/flows/{result.Value!.Id}", result.Value)
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "ApprovalFlow_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] SaveApprovalFlowDefinitionRequest request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Approval flow updated successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "ApprovalFlow_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? Success(result.Value, "Approval flow deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }
}
