using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Modules.Inventory.Queries;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Controllers;

[Route("api/inventory/quarantine-holds")]
[Authorize]
public class QuarantineHoldsController(CrudUseCase<QuarantineHold> useCase, InventoryAdjacentQuery adjacentQuery) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "QuarantineHold_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}/adjacent")]
    [Authorize(Policy = "QuarantineHold_View")]
    public async Task<IActionResult> GetAdjacent(long id) => Success(await adjacentQuery.GetQuarantineHoldAdjacentAsync(id));

    [HttpGet("{id:long}")]
    [Authorize(Policy = "QuarantineHold_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Quarantine hold not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "QuarantineHold_Create")]
    public async Task<IActionResult> Create([FromBody] QuarantineHold request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/inventory/quarantine-holds/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "QuarantineHold_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] QuarantineHold request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Quarantine hold updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "QuarantineHold_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Quarantine hold deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
