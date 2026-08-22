using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Inventory.Dtos;
using Sugentra.ERP.Api.Modules.Inventory.UseCases;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Controllers;

[Route("api/inventory/stock-opnames")]
[Authorize]
public class StockOpnamesController(StockOpnameUseCase useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "StockOpname_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

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

    [HttpPost("{id:long}/submit")]
    [Authorize(Policy = "StockOpname_Edit")]
    public async Task<IActionResult> Submit(long id)
    {
        var result = await useCase.SubmitAsync(id);
        return result.IsSuccess
            ? Success(result.Value, "Stock opname submitted successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpPost("{id:long}/approve")]
    [Authorize(Policy = "StockOpname_Edit")]
    public async Task<IActionResult> Approve(long id)
    {
        var result = await useCase.ApproveAsync(id);
        return result.IsSuccess
            ? Success(result.Value, "Stock opname approved successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

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
