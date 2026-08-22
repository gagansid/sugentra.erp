using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.MasterData.Dtos;
using Sugentra.ERP.Api.Modules.MasterData.Queries;
using Sugentra.ERP.Api.Modules.MasterData.UseCases;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.MasterData.Controllers;

[Route("api/master-data/price-lists")]
[Authorize]
public class PriceListsController(PriceListUseCase useCase, PriceListListQuery listQuery) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "MasterData_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("paged")]
    [Authorize(Policy = "MasterData_View")]
    public async Task<IActionResult> GetPaged([FromQuery] PriceListListRequest request) => Success(await listQuery.GetPagedAsync(request));

    [HttpGet("{id:long}/adjacent")]
    [Authorize(Policy = "MasterData_View")]
    public async Task<IActionResult> GetAdjacent(long id) => Success(await listQuery.GetAdjacentAsync(id));

    [HttpGet("{id:long}")]
    [Authorize(Policy = "MasterData_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var response = await useCase.GetByIdAsync(id);
        return response is null ? Failure("Price list not found.", StatusCodes.Status404NotFound) : Success(response);
    }

    [HttpPost]
    [Authorize(Policy = "MasterData_Create")]
    public async Task<IActionResult> Create([FromBody] CreatePriceListRequest request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/master-data/price-lists/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "MasterData_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePriceListRequest request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Price list updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "MasterData_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Price list deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
