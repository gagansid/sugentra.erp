namespace Sugentra.ERP.UI.Models;

/// <summary>Mirrors Sugentra.ERP.Api.Shared.Common.ApiResponse&lt;T&gt; for deserializing API responses.</summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public object? Errors { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>Mirrors Sugentra.ERP.Api.Shared.Common.PagedResult&lt;T&gt;.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
