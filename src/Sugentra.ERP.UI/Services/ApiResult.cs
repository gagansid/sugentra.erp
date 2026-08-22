namespace Sugentra.ERP.UI.Services;

/// <summary>Normalized result of an API call — Message always comes from the API's ApiResponse envelope.</summary>
public class ApiResult<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public object? Errors { get; init; }
    public int StatusCode { get; init; }
    public bool Unauthorized => StatusCode == 401;
}
