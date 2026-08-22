using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Controllers;

[Route("api/inventory/landed-cost-allocations")]
[Authorize]
public class LandedCostAllocationsController(CrudUseCase<LandedCostAllocation> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Batch_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Batch_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Landed cost allocation not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "Batch_Create")]
    public async Task<IActionResult> Create([FromBody] LandedCostAllocation request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/inventory/landed-cost-allocations/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Batch_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] LandedCostAllocation request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Landed cost allocation updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Batch_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Landed cost allocation deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
