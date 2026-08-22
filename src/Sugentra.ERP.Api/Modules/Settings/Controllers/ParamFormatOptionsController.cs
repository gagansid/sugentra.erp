using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

// Read-only by design - rows are seeded/managed directly in the database, no CRUD UI needed.
[Route("api/settings/param-format-options")]
[Authorize]
public class ParamFormatOptionsController(CrudUseCase<ParamFormatOption> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "ParamFormatOption_View")]
    public async Task<IActionResult> GetAll([FromQuery] string? dataType)
    {
        var all = await useCase.GetAllAsync();
        var result = (string.IsNullOrWhiteSpace(dataType) ? all : all.Where(o => o.DataType == dataType))
            .OrderBy(o => o.SortOrder)
            .ToList();
        return Success(result);
    }
}
