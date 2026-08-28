using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/holidays")]
[Authorize]
public class HolidaysController(CrudUseCase<Holiday> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Holiday_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Holiday_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Holiday not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "Holiday_Create")]
    public async Task<IActionResult> Create([FromBody] Holiday request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/settings/holidays/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Holiday_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] Holiday request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Holiday updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Holiday_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Holiday deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
