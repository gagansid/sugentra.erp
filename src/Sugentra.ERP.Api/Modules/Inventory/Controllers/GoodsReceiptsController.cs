using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Inventory.Dtos;
using Sugentra.ERP.Api.Modules.Inventory.Queries;
using Sugentra.ERP.Api.Modules.Inventory.UseCases;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Inventory.Controllers;

[Route("api/inventory/goods-receipts")]
[Authorize]
public class GoodsReceiptsController(GoodsReceiptUseCase useCase, GoodsReceiptListQuery listQuery, IApprovalService approvalService) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "GoodsReceipt_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}/adjacent")]
    [Authorize(Policy = "GoodsReceipt_View")]
    public async Task<IActionResult> GetAdjacent(long id) => Success(await listQuery.GetAdjacentAsync(id));

    [HttpGet("{id:long}/approval-history")]
    [Authorize(Policy = "GoodsReceipt_View")]
    public async Task<IActionResult> GetApprovalHistory(long id) => Success(await approvalService.GetHistoryAsync("GoodsReceipt", id));

    [HttpGet("{id:long}")]
    [Authorize(Policy = "GoodsReceipt_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var response = await useCase.GetByIdAsync(id);
        return response is null ? Failure("Goods receipt not found.", StatusCodes.Status404NotFound) : Success(response);
    }

    [HttpPost]
    [Authorize(Policy = "GoodsReceipt_Create")]
    public async Task<IActionResult> Create([FromBody] CreateGoodsReceiptRequest request)
    {
        var result = await useCase.CreateAsync(request);
        return result.IsSuccess
            ? SuccessCreated($"api/inventory/goods-receipts/{result.Value!.Id}", result.Value)
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "GoodsReceipt_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateGoodsReceiptRequest request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Goods receipt updated successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpPost("{id:long}/post")]
    [Authorize(Policy = "GoodsReceipt_Edit")]
    public async Task<IActionResult> Post(long id)
    {
        var result = await useCase.PostAsync(id);
        return result.IsSuccess
            ? Success(result.Value, "Goods receipt posted successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "GoodsReceipt_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Goods receipt deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }
}
