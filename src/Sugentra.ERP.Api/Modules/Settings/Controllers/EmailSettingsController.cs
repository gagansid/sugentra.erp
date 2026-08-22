using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Dtos;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Modules.Settings.Services;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/email-settings")]
[Authorize]
public class EmailSettingsController(
    CrudUseCase<EmailSetting> useCase,
    GenericRepository<EmailSetting> repository,
    EmailConnectionTestService testService) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "EmailSetting_View")]
    public async Task<IActionResult> GetAll() => Success(await useCase.GetAllAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "EmailSetting_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await useCase.GetByIdAsync(id);
        return entity is null ? Failure("Email setting not found.", StatusCodes.Status404NotFound) : Success(entity);
    }

    [HttpPost]
    [Authorize(Policy = "EmailSetting_Create")]
    public async Task<IActionResult> Create([FromBody] EmailSetting request)
    {
        var created = await useCase.CreateAsync(request);
        return SuccessCreated($"api/settings/email-settings/{created.Id}", created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "EmailSetting_Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] EmailSetting request)
    {
        // Password input never round-trips its value; keep the stored password when the field is left blank.
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            var existing = await repository.GetByIdAsync(id);
            if (existing is not null)
            {
                request.Password = existing.Password;
            }
        }

        var result = await useCase.UpdateAsync(id, request);
        return result.IsSuccess
            ? Success(result.Value, "Email setting updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "EmailSetting_Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await useCase.DeleteAsync(id);
        return result.IsSuccess
            ? SuccessMessage("Email setting deleted successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }

    [HttpPost("test")]
    [Authorize(Policy = "EmailSetting_Edit")]
    public async Task<IActionResult> Test([FromBody] TestEmailSettingRequest request)
    {
        var settings = request.Settings;

        // Password inputs never round-trip their value; fall back to the stored password when left blank on an existing record.
        EmailSetting? existing = null;
        if (settings.Id > 0)
        {
            existing = await repository.GetByIdAsync(settings.Id);
            if (existing is not null && string.IsNullOrWhiteSpace(settings.Password))
            {
                settings.Password = existing.Password;
            }
        }

        var result = await testService.TestAsync(settings, request.ToEmail, request.Subject, request.Body);

        if (existing is not null)
        {
            existing.LastTestedAt = DateTime.UtcNow;
            existing.LastTestStatus = result.IsSuccess ? "Success" : "Failed";
            existing.LastTestMessage = result.IsSuccess ? null : result.Error;
            await repository.UpdateAsync(existing);
        }

        return result.IsSuccess
            ? Success(true, "Test email sent successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }
}
