namespace Sugentra.ERP.UI.Models.Identity;

public record PermissionResponse(long Id, string Code, string? Module, string? Description, DateTime CreatedAt);

public record PermissionListItemDto(long Id, string Code, string? Module, string? Description);

public record PermissionListRequest(string? Code = null, string? Module = null, int Page = 1, int PageSize = 10);
