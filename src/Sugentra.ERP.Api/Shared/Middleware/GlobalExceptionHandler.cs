using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.ErrorLogging;

namespace Sugentra.ERP.Api.Shared.Middleware;

/// <summary>Catches any unhandled exception, records it to Shared_ErrorLogs for later audit, and returns a
/// uniform ApiResponse 500 instead of leaking a stack trace to the caller.</summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);

        // IExceptionHandler is registered as a singleton, but IErrorLogService is scoped - resolve it from
        // the current request's DI scope instead of constructor injection.
        var errorLogService = httpContext.RequestServices.GetRequiredService<IErrorLogService>();
        await errorLogService.LogAsync(new ErrorLogEntry(
            Source: "Api",
            Endpoint: httpContext.Request.Path,
            HttpMethod: httpContext.Request.Method,
            StatusCode: StatusCodes.Status500InternalServerError,
            ExceptionType: exception.GetType().FullName,
            Message: exception.Message,
            StackTrace: exception.StackTrace,
            QueryString: httpContext.Request.QueryString.HasValue ? httpContext.Request.QueryString.Value : null,
            RequestParameters: null,
            UserId: long.TryParse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null,
            Username: httpContext.User.FindFirstValue(ClaimTypes.Name),
            IpAddress: httpContext.Connection.RemoteIpAddress?.ToString()));

        var response = new ApiResponse<object?>
        {
            Success = false,
            Message = "An unexpected error occurred."
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}

