using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/ports")]
[Authorize]
public class PortsController(CrudUseCase<Port> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Port_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Port_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Port not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "Port_Create")]
    public async Task<IActionResult> Create([FromBody] Port request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/settings/ports/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Port_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] Port request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Port updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Port_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Port deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
