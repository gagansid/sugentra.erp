using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Inventory.Dtos;
using Sugentra.ERP.Api.Modules.Inventory.Queries;
using Sugentra.ERP.Api.Modules.Inventory.UseCases;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Inventory.Controllers;

[Route("api/inventory/stock-opnames")]
[Authorize]
public class StockOpnamesController(StockOpnameUseCase useCase, InventoryAdjacentQuery adjacentQuery, IApprovalService approvalService) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "StockOpname_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}/adjacent")]
    [Authorize(Policy = "StockOpname_View")]
    public async Task<IActionResult> GetAdjacent(long id) => Success(await adjacentQuery.GetStockOpnameAdjacentAsync(id));

    [HttpGet("{id:long}")]
    [Authorize(Policy = "StockOpname_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var response = await useCase.GetByIdAsync(id);
        return response is null ? Failure("Stock opname not found.", StatusCodes.Status404NotFound) : Success(response);
    }

    [HttpPost]
    [Authorize(Policy = "StockOpname_Create")]
    public async Task<IActionResult> Create([FromBody] CreateStockOpnameRequest request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/inventory/stock-opnames/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "StockOpname_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateStockOpnameRequest request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Stock opname updated successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpPost("{id:long}/post")]
    [Authorize(Policy = "StockOpname_Edit")]
    public async Task<IActionResult> Post(long id)
    {
        var result = await useCase.PostAsync(id);
        return result.IsSuccess
            ? Success(result.Value, "Stock opname posted successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpGet("{id:long}/approval-history")]
    [Authorize(Policy = "StockOpname_View")]
    public async Task<IActionResult> GetApprovalHistory(long id) => Success(await approvalService.GetHistoryAsync("StockOpname", id));

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "StockOpname_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Stock opname deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }
}
