using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/currencies")]
[Authorize]
public class CurrenciesController(CrudUseCase<Currency> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Currency_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Currency_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Currency not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "Currency_Create")]
    public async Task<IActionResult> Create([FromBody] Currency request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/settings/currencies/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Currency_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] Currency request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Currency updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Currency_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Currency deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
