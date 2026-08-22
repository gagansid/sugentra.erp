using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Modules.Settings.Services;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/document-numberings")]
[Authorize]
public class DocumentNumberingsController(CrudUseCase<DocumentNumbering> useCase, DocumentNumberGeneratorService generatorService) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "DocumentNumbering_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "DocumentNumbering_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Document numbering config not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "DocumentNumbering_Create")]
    public async Task<IActionResult> Create([FromBody] DocumentNumbering request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/settings/document-numberings/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "DocumentNumbering_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] DocumentNumbering request)
    {
        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Document numbering config updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "DocumentNumbering_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Document numbering config deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    // Consumed by future modules (Sales/Export Documentation) to reserve the next running number for a document type.
    [HttpPost("{documentType}/next")]
    [Authorize(Policy = "DocumentNumbering_Edit")]
    public async Task<IActionResult> GetNext(string documentType)
    {
        try
        {
            var result = await generatorService.GetNextAsync(documentType);
            return Success(result);
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ex.Message, StatusCodes.Status404NotFound);
        }
    }
}
