namespace Sugentra.ERP.UI.Models.Layout;

/// <summary>Shape needed by the shared paging partials — matches PagedResult&lt;T&gt;'s paging fields.</summary>
public record PagingInfo(int Page, int PageSize, int TotalCount, int TotalPages)
{
    public static PagingInfo From<T>(PagedResult<T> result) =>
        new(result.Page, result.PageSize, result.TotalCount, result.TotalPages);
}
