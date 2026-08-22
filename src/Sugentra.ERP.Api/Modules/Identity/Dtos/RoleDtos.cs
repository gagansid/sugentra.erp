namespace Sugentra.ERP.Api.Modules.Identity.Dtos;

public record CreateRoleRequest(string Name, string? Description, bool IsActive = true);

public record UpdateRoleRequest(string Name, string? Description, bool IsActive = true);

public record RoleResponse(long Id, string Name, string? Description, bool IsActive, DateTime CreatedAt);

public record RoleListItemDto(long Id, string Name, string? Description, bool IsActive);

// Bound as one model from query string (?Name=&Page=&PageSize=) — filter is optional.
public record RoleListRequest(string? Name = null, int Page = 1, int PageSize = 20);
