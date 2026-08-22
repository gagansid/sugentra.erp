using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/units-of-measurement")]
[Authorize]
public class UnitsOfMeasurementController(CrudUseCase<UnitOfMeasurement> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "UnitOfMeasurement_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "UnitOfMeasurement_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Unit of measurement not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "UnitOfMeasurement_Create")]
    public async Task<IActionResult> Create([FromBody] UnitOfMeasurement request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/settings/units-of-measurement/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "UnitOfMeasurement_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] UnitOfMeasurement request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Unit of measurement updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "UnitOfMeasurement_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Unit of measurement deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
