namespace Sugentra.ERP.Api.Modules.Identity.Dtos;

public record UserRoleSummaryDto(long Id, string Name);

public record EffectivePermissionDto(string Code, string? Module, string? Description, string Source, DateTime? ModifiedAt = null);

public record UserRolesPermissionsResponse(
    IReadOnlyList<UserRoleSummaryDto> Roles,
    IReadOnlyList<EffectivePermissionDto> EffectivePermissions,
    IReadOnlyList<EffectivePermissionDto> DeniedOverrides);

public record UserSessionDto(long Id, string? DeviceInfo, string? IpAddress, DateTime CreatedAt, DateTime ExpiresAt, DateTime? RevokedAt);
