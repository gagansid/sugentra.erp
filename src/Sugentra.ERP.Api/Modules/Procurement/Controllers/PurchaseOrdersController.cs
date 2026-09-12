using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Procurement.Dtos;
using Sugentra.ERP.Api.Modules.Procurement.Queries;
using Sugentra.ERP.Api.Modules.Procurement.UseCases;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Procurement.Controllers;

[Route("api/procurement/purchase-orders")]
[Authorize]
public class PurchaseOrdersController(PurchaseOrderUseCase useCase, ProcurementAdjacentQuery adjacentQuery, IApprovalService approvalService) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "PurchaseOrder_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}/adjacent")]
    [Authorize(Policy = "PurchaseOrder_View")]
    public async Task<IActionResult> GetAdjacent(long id) => Success(await adjacentQuery.GetPurchaseOrderAdjacentAsync(id));

    [HttpGet("{id:long}/approval-history")]
    [Authorize(Policy = "PurchaseOrder_View")]
    public async Task<IActionResult> GetApprovalHistory(long id) => Success(await approvalService.GetHistoryAsync("PurchaseOrder", id));

    [HttpGet("{id:long}")]
    [Authorize(Policy = "PurchaseOrder_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var response = await useCase.GetByIdAsync(id);
        return response is null ? Failure("Purchase order not found.", StatusCodes.Status404NotFound) : Success(response);
    }

    [HttpPost]
    [Authorize(Policy = "PurchaseOrder_Create")]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest request)
    {
        var result = await useCase.CreateAsync(request);
        return result.IsSuccess
            ? SuccessCreated($"api/procurement/purchase-orders/{result.Value!.Id}", result.Value)
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "PurchaseOrder_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePurchaseOrderRequest request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Purchase order updated successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpPost("{id:long}/submit")]
    [Authorize(Policy = "PurchaseOrder_Approve")]
    public async Task<IActionResult> Submit(long id)
    {
        var result = await useCase.SubmitAsync(id);
        return result.IsSuccess
            ? Success(result.Value, "Purchase order submitted successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "PurchaseOrder_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Purchase order deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }
}
