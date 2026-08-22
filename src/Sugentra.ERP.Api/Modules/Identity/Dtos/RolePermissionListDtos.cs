namespace Sugentra.ERP.Api.Modules.Identity.Dtos;

public record RolePermissionListItemDto(long Id, string Code, string? Module, string? Description, bool IsAssigned, DateTime? ModifiedAt);

// Bound as one model from query string (?Module=&Code=&Description=&IsAssigned=&Page=&PageSize=) — all filters optional.
public record RolePermissionListRequest(string? Module = null, string? Code = null, string? Description = null, bool? IsAssigned = null, int Page = 1, int PageSize = 10);
