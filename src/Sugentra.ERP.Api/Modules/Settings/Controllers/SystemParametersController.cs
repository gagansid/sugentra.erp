using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Modules.Settings.Queries;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/system-parameters")]
[Authorize]
public class SystemParametersController(CrudUseCase<SystemParameter> useCase, SystemParameterListQuery listQuery) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "SystemParameter_View")]
    public async Task<IActionResult> GetAll([FromQuery] string? category)
    {
        var all = await useCase.GetAllAsync();
        var result = string.IsNullOrWhiteSpace(category) ? all : all.Where(p => p.ParamCategory == category).ToList();
        return Success(result);
    }

    [HttpGet("paged")]
    [Authorize(Policy = "SystemParameter_View")]
    public async Task<IActionResult> GetPaged([FromQuery] SystemParameterListRequest request) =>
        Success(await listQuery.GetPagedAsync(request));

    [HttpGet("{id:long}")]
    [Authorize(Policy = "SystemParameter_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("System parameter not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "SystemParameter_Create")]
    public async Task<IActionResult> Create([FromBody] SystemParameter request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/settings/system-parameters/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "SystemParameter_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] SystemParameter request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "System parameter updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "SystemParameter_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("System parameter deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
