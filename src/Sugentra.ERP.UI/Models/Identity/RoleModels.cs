namespace Sugentra.ERP.UI.Models.Identity;

public record RoleResponse(long Id, string Name, string? Description, bool IsActive, DateTime CreatedAt);

public record RoleListItemDto(long Id, string Name, string? Description, bool IsActive);

public record RoleListRequest(string? Name = null, int Page = 1, int PageSize = 10);

public record CreateRoleRequest(string Name, string? Description, bool IsActive = true);

public record UpdateRoleRequest(string Name, string? Description, bool IsActive = true);

public record RolePermissionListItemDto(long Id, string Code, string? Module, string? Description, bool IsAssigned, DateTime? ModifiedAt);

public record RolePermissionListRequest(string? Module = null, string? Code = null, string? Description = null, bool? IsAssigned = null, int Page = 1, int PageSize = 10);
