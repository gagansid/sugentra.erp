using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Inventory.Dtos;
using Sugentra.ERP.Api.Modules.Inventory.UseCases;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Controllers;

[Route("api/inventory/stock-mutations")]
[Authorize]
public class StockMutationsController(StockMutationUseCase useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "StockMutation_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "StockMutation_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var response = await useCase.GetByIdAsync(id);
        return response is null ? Failure("Stock mutation not found.", StatusCodes.Status404NotFound) : Success(response);
    }

    [HttpPost]
    [Authorize(Policy = "StockMutation_Create")]
    public async Task<IActionResult> Create([FromBody] CreateStockMutationRequest request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/inventory/stock-mutations/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "StockMutation_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateStockMutationRequest request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Stock mutation updated successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpPost("{id:long}/approve")]
    [Authorize(Policy = "StockMutation_Edit")]
    public async Task<IActionResult> Approve(long id)
    {
        var result = await useCase.ApproveAsync(id);
        return result.IsSuccess
            ? Success(result.Value, "Stock mutation approved successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpPost("{id:long}/complete")]
    [Authorize(Policy = "StockMutation_Edit")]
    public async Task<IActionResult> Complete(long id)
    {
        var result = await useCase.CompleteAsync(id);
        return result.IsSuccess
            ? Success(result.Value, "Stock mutation completed successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "StockMutation_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Stock mutation deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }
}
