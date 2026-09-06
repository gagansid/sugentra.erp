using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Inventory.Dtos;
using Sugentra.ERP.Api.Modules.Inventory.UseCases;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Controllers;

[Route("api/inventory/landed-cost-documents")]
[Authorize]
public class LandedCostDocumentsController(LandedCostDocumentUseCase useCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Batch_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Batch_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var response = await useCase.GetByIdAsync(id);
        return response is null ? Failure("Landed cost document not found.", StatusCodes.Status404NotFound) : Success(response);
    }

    [HttpPost]
    [Authorize(Policy = "Batch_Create")]
    public async Task<IActionResult> Create([FromBody] CreateLandedCostDocumentRequest request)
    {
        var result = await useCase.CreateAsync(request);
        return result.IsSuccess
            ? SuccessCreated($"api/inventory/landed-cost-documents/{result.Value!.Id}", result.Value)
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpPost("{id:long}/post")]
    [Authorize(Policy = "Batch_Edit")]
    public async Task<IActionResult> Post(long id)
    {
        var result = await useCase.PostAsync(id);
        return result.IsSuccess
            ? Success(result.Value, "Landed cost document posted successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }
}
