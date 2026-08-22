using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Controllers;

// Read-only: StockLedgers is an immutable movement audit trail (kartu stok), append-only by the stock flows.
[Route("api/inventory/stock-ledgers")]
[Authorize]
public class StockLedgersController(CrudUseCase<StockLedger> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "StockLedger_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "StockLedger_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Stock ledger entry not found.", StatusCodes.Status404NotFound) : Success(entity);
    }
}
