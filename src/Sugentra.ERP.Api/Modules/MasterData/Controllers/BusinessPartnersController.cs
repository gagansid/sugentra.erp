using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.MasterData.Dtos;
using Sugentra.ERP.Api.Modules.MasterData.Queries;
using Sugentra.ERP.Api.Modules.MasterData.UseCases;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.MasterData.Controllers;

[Route("api/master-data/business-partners")]
[Authorize]
public class BusinessPartnersController(BusinessPartnerUseCase useCase, BusinessPartnerListQuery listQuery) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "MasterData_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("paged")]
    [Authorize(Policy = "MasterData_View")]
    public async Task<IActionResult> GetPaged([FromQuery] BusinessPartnerListRequest request) => Success(await listQuery.GetPagedAsync(request));

    [HttpGet("{id:long}/adjacent")]
    [Authorize(Policy = "MasterData_View")]
    public async Task<IActionResult> GetAdjacent(long id) => Success(await listQuery.GetAdjacentAsync(id));

    [HttpGet("{id:long}")]
    [Authorize(Policy = "MasterData_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var response = await useCase.GetByIdAsync(id);
        return response is null ? Failure("Business partner not found.", StatusCodes.Status404NotFound) : Success(response);
    }

    [HttpPost]
    [Authorize(Policy = "MasterData_Create")]
    public async Task<IActionResult> Create([FromBody] CreateBusinessPartnerRequest request)
    {
        var result = await useCase.CreateAsync(request);
        return result.IsSuccess
            ? SuccessCreated($"api/master-data/business-partners/{result.Value!.Id}", result.Value)
            : Failure(result.Error!, StatusCodes.Status422UnprocessableEntity);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "MasterData_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateBusinessPartnerRequest request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Business partner updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "MasterData_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Business partner deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
