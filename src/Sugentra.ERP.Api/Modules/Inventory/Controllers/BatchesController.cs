using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Modules.Inventory.UseCases;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Controllers;

[Route("api/inventory/batches")]
[Authorize]
public class BatchesController(BatchUseCase useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Batch_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Batch_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Batch not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "Batch_Create")]
    public async Task<IActionResult> Create([FromBody] Batch request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/inventory/batches/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Batch_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] Batch request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Batch updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Batch_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Batch deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }
}
