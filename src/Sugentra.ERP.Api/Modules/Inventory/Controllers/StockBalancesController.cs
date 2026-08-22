using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Controllers;

// Read-only: StockBalances is a system-computed snapshot (updated by Receipt/Mutation/Consumption/Opname
// flows), never edited directly by a user.
[Route("api/inventory/stock-balances")]
[Authorize]
public class StockBalancesController(CrudUseCase<StockBalance> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "StockBalance_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "StockBalance_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Stock balance not found.", StatusCodes.Status404NotFound) : Success(entity);
    }
}
