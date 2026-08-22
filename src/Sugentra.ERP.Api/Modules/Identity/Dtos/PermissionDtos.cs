namespace Sugentra.ERP.Api.Modules.Identity.Dtos;

public record CreatePermissionRequest(string Code, string? Module, string? Description);

// Code is immutable once created (it's the policy/claim name) — only Module/Description can change.
public record UpdatePermissionRequest(string? Module, string? Description);

public record PermissionResponse(long Id, string Code, string? Module, string? Description, DateTime CreatedAt);

public record PermissionListItemDto(long Id, string Code, string? Module, string? Description);

// Bound as one model from query string (?Code=&Module=&Page=&PageSize=) — filters are optional.
public record PermissionListRequest(string? Code = null, string? Module = null, int Page = 1, int PageSize = 20);
