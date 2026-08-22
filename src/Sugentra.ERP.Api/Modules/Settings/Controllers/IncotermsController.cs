using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/incoterms")]
[Authorize]
public class IncotermsController(CrudUseCase<Incoterm> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Incoterm_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Incoterm_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Incoterm not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "Incoterm_Create")]
    public async Task<IActionResult> Create([FromBody] Incoterm request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/settings/incoterms/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Incoterm_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] Incoterm request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Incoterm updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Incoterm_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Incoterm deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
