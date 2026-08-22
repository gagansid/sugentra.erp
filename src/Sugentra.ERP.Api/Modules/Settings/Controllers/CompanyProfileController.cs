using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

// Singleton config record (always Id=1, seeded by the Migrator) — only Get/Update are exposed, no Create/Delete.
[Route("api/settings/company-profile")]
[Authorize]
public class CompanyProfileController(CrudUseCase<CompanyProfile> useCase) : ApiControllerBase
{
    private const long SingletonId = 1;

    [HttpGet]
    [Authorize(Policy = "CompanyProfile_View")]
    public async Task<IActionResult> Get()
    {
        var entity = await useCase.GetByIdAsync(SingletonId);
        return entity is null ? Failure("Company profile not configured.", StatusCodes.Status404NotFound) : Success(entity);
    }

    // Branding only (name/logo) - exposed to any authenticated user so it can be shown on the dashboard,
    // unlike the full Get above which requires CompanyProfile_View since it also returns address/tax/contact info.
    [HttpGet("identity")]
    [Authorize]
    public async Task<IActionResult> GetIdentity()
    {
        var entity = await useCase.GetByIdAsync(SingletonId);
        return entity is null
            ? Failure("Company profile not configured.", StatusCodes.Status404NotFound)
            : Success(new { entity.CompanyName, entity.LogoUrl });
    }

    // Same branding-only payload as GetIdentity, but reachable pre-login (e.g. the sign-in page logo).
    [HttpGet("branding")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBranding()
    {
        var entity = await useCase.GetByIdAsync(SingletonId);
        return entity is null
            ? Failure("Company profile not configured.", StatusCodes.Status404NotFound)
            : Success(new { entity.CompanyName, entity.LogoUrl });
    }

    [HttpPut]
    [Authorize(Policy = "CompanyProfile_Edit")]
    public async Task<IActionResult> Update([FromBody] CompanyProfile request)
    {
        var result = await useCase.UpdateAsync(SingletonId, request);
        return result.IsSuccess
            ? Success(result.Value, "Company profile updated successfully.")
            : Failure(result.Error!, StatusCodes.Status404NotFound);
    }
}
