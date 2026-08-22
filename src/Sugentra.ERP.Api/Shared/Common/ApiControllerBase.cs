using Microsoft.AspNetCore.Mvc;

namespace Sugentra.ERP.Api.Shared.Common;

/// <summary>
/// Base controller enforcing one consistent <see cref="ApiResponse{T}"/> envelope for every response, success or error.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Success<T>(T data, string? message = null, int statusCode = StatusCodes.Status200OK) =>
        StatusCode(statusCode, new ApiResponse<T> { Success = true, Data = data, Message = message });

    protected IActionResult SuccessCreated<T>(string location, T data, string? message = null) =>
        Created(location, new ApiResponse<T> { Success = true, Data = data, Message = message });

    // For actions with nothing to return (e.g. Delete/Deactivate acknowledgements).
    protected IActionResult SuccessMessage(string? message = null, int statusCode = StatusCodes.Status200OK) =>
        StatusCode(statusCode, new ApiResponse<object?> { Success = true, Message = message });

    protected IActionResult Failure(string message, int statusCode = StatusCodes.Status400BadRequest, object? errors = null) =>
        StatusCode(statusCode, new ApiResponse<object?> { Success = false, Message = message, Errors = errors });

    protected IActionResult ValidationFailure(string message, object errors) =>
        Failure(message, StatusCodes.Status422UnprocessableEntity, errors);
}
