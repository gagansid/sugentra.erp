using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/warehouses")]
[Authorize]
public class WarehousesController(CrudUseCase<Warehouse> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Warehouse_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Warehouse_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Warehouse not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "Warehouse_Create")]
    public async Task<IActionResult> Create([FromBody] Warehouse request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/settings/warehouses/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Warehouse_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] Warehouse request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Warehouse updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Warehouse_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Warehouse deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
