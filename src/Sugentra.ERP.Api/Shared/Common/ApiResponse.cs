namespace Sugentra.ERP.Api.Shared.Common;

/// <summary>Uniform envelope for every response (success and error) across all modules/controllers.</summary>
public class ApiResponse<T>
{
    public bool Success { get; init; } = true;
    public string? Message { get; init; }
    public T? Data { get; init; }
    public object? Errors { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
