using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Controllers;

// Read-only: allocations are now computed by LandedCostDocumentUseCase.PostAsync, not entered manually.
[Route("api/inventory/landed-cost-allocations")]
[Authorize]
public class LandedCostAllocationsController(CrudUseCase<LandedCostAllocation> useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Batch_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Batch_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Landed cost allocation not found.", StatusCodes.Status404NotFound) : Success(entity);
    }
}

