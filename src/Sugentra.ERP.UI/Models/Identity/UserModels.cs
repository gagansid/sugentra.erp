namespace Sugentra.ERP.UI.Models.Identity;

public record LoginRequest(string Username, string Password);

public record TokenResponse(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt);

public record UserResponse(long Id, string Username, string Email, string FullName, bool IsActive, string? PhoneNumber, string? EmployeeId, string? ProfilePictureUrl, DateTime? LastLoginAt, DateTime CreatedAt, DateTime? EmailVerifiedAt = null, DateTime? PhoneVerifiedAt = null);

public record UserListItemDto(long Id, string Username, string Email, string FullName, bool IsActive, DateTime? LockoutEnd, bool IsBanned);

public record UserListRequest(string? Username = null, string? Email = null, string? FullName = null, string? Status = null, int Page = 1, int PageSize = 10);

public record CreateUserRequest(string Username, string Email, string Password, string FullName, string? PhoneNumber = null, string? EmployeeId = null);

public record UpdateUserRequest(string Email, string FullName, bool IsActive, string? PhoneNumber = null, string? EmployeeId = null, string? ProfilePictureUrl = null);

public record ChangePasswordRequest(string NewPassword);

public record UserRoleSummaryDto(long Id, string Name);

public record EffectivePermissionDto(string Code, string? Module, string? Description, string Source, DateTime? ModifiedAt = null);

public record UserRolesPermissionsResponse(
    IReadOnlyList<UserRoleSummaryDto> Roles,
    IReadOnlyList<EffectivePermissionDto> EffectivePermissions,
    IReadOnlyList<EffectivePermissionDto> DeniedOverrides);

public record UserSessionDto(long Id, string? DeviceInfo, string? IpAddress, DateTime CreatedAt, DateTime ExpiresAt, DateTime? RevokedAt);

public class UserDetailViewModel
{
    public required UserResponse User { get; init; }
    public required UserRolesPermissionsResponse RolesPermissions { get; init; }
    public required PagedResult<UserSessionDto> Sessions { get; init; }
    public required PagedResult<Sugentra.ERP.UI.Models.Settings.AuditLogListItemDto> AuditLogs { get; init; }
    public required IReadOnlyList<RoleListItemDto> AllRoles { get; init; }
    public required PagedResult<PermissionListItemDto> PermissionCatalog { get; init; }
    public required IReadOnlyList<Sugentra.ERP.UI.Models.Settings.Module> PermissionModules { get; init; }
    public string? PermModule { get; init; }
    public string? PermCode { get; init; }
}
