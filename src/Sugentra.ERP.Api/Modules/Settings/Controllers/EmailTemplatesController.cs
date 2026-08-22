using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/email-templates")]
[Authorize]
public class EmailTemplatesController(CrudUseCase<EmailTemplate> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "EmailTemplate_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "EmailTemplate_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Email template not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "EmailTemplate_Create")]
    public async Task<IActionResult> Create([FromBody] EmailTemplate request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/settings/email-templates/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "EmailTemplate_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] EmailTemplate request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Email template updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "EmailTemplate_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Email template deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
