using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Models.Shared;
using Sugentra.ERP.UI.Services.Shared;

namespace Sugentra.ERP.UI.Controllers.Shared;

// Target of app.UseExceptionHandler("/Error") - never shows the raw exception to the user, only a generic
// "something went wrong, contact the developer" page. The real details are sent to the API's error log instead.
[AllowAnonymous]
[Route("Error")]
public class ErrorController(ErrorLogApiService errorLogApiService, ILogger<ErrorController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var exception = feature?.Error;

        if (exception is not null)
        {
            logger.LogError(exception, "Unhandled UI exception on {Path}", feature!.Path);

            var entry = new ErrorLogEntry(
                Source: "UI",
                Endpoint: feature.Path,
                HttpMethod: Request.Method,
                StatusCode: 500,
                ExceptionType: exception.GetType().FullName,
                Message: exception.Message,
                StackTrace: exception.StackTrace,
                QueryString: Request.QueryString.HasValue ? Request.QueryString.Value : null,
                RequestParameters: null,
                UserId: null,
                Username: User.Identity?.Name,
                IpAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

            try
            {
                await errorLogApiService.ReportAsync(entry);
            }
            catch
            {
                // Best-effort only - never let error-log reporting itself crash the error page.
            }
        }

        Response.StatusCode = 500;
        return View();
    }
}
