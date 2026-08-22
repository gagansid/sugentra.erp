using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Uploads;

namespace Sugentra.ERP.Api.Controllers;

// Generic upload endpoint shared by every module - validation rules per category live server-side in UploadCategories,
// the caller only picks a category, it cannot loosen the size/extension limits.
[Route("api/uploads")]
[Authorize]
public class UploadsController(IFileStorageService fileStorageService) : ApiControllerBase
{
    [HttpPost]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string category, [FromForm] string? entityType = null, [FromForm] long? entityId = null, [FromForm] string? replaceUrl = null)
    {
        try
        {
            var upload = await fileStorageService.SaveAsync(file, category, entityType, entityId, replaceUrl);
            return Success(upload, "File uploaded successfully.");
        }
        catch (FileValidationException ex)
        {
            return Failure(ex.Message);
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await fileStorageService.DeleteAsync(id);
        return SuccessMessage("File deleted successfully.");
    }
}
