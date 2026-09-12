using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Procurement.Dtos;
using Sugentra.ERP.Api.Modules.Procurement.Queries;
using Sugentra.ERP.Api.Modules.Procurement.UseCases;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Procurement.Controllers;

[Route("api/procurement/purchase-requisitions")]
[Authorize]
public class PurchaseRequisitionsController(PurchaseRequisitionUseCase useCase, ProcurementAdjacentQuery adjacentQuery, IApprovalService approvalService) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "PurchaseRequisition_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}/adjacent")]
    [Authorize(Policy = "PurchaseRequisition_View")]
    public async Task<IActionResult> GetAdjacent(long id) => Success(await adjacentQuery.GetPurchaseRequisitionAdjacentAsync(id));

    [HttpGet("{id:long}/approval-history")]
    [Authorize(Policy = "PurchaseRequisition_View")]
    public async Task<IActionResult> GetApprovalHistory(long id) => Success(await approvalService.GetHistoryAsync("PurchaseRequisition", id));

    [HttpGet("{id:long}")]
    [Authorize(Policy = "PurchaseRequisition_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var response = await useCase.GetByIdAsync(id);
        return response is null ? Failure("Purchase requisition not found.", StatusCodes.Status404NotFound) : Success(response);
    }

    [HttpPost]
    [Authorize(Policy = "PurchaseRequisition_Create")]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseRequisitionRequest request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/procurement/purchase-requisitions/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "PurchaseRequisition_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePurchaseRequisitionRequest request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Purchase requisition updated successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpPost("{id:long}/submit")]
    [Authorize(Policy = "PurchaseRequisition_Approve")]
    public async Task<IActionResult> Submit(long id)
    {
        var result = await useCase.SubmitAsync(id);
        return result.IsSuccess
            ? Success(result.Value, "Purchase requisition submitted successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "PurchaseRequisition_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Purchase requisition deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }
}
