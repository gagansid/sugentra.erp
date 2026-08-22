using System.Text.Json;
using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Repositories;
using Sugentra.ERP.Api.Modules.Identity.Services;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Logging;

namespace Sugentra.ERP.Api.Modules.Identity.UseCases;

public class UserUseCase(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository,
    IPermissionRepository permissionRepository,
    IUserPermissionRepository userPermissionRepository,
    IUserRefreshTokenRepository refreshTokenRepository,
    PermissionResolverService permissionResolverService,
    IPasswordHasher passwordHasher,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    // Read path: Controller talks only to UseCase, which delegates to Repository internally (keeps the Controller uniform).
    public Task<PagedResult<UserListItemDto>> GetPagedAsync(UserListRequest request) =>
        userRepository.GetPagedAsync(request);

    public async Task<UserResponse?> GetByIdAsync(long id)
    {
        var user = await userRepository.GetByIdAsync(id);
        return user is null ? null : ToResponse(user);
    }

    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request)
    {
        if (await userRepository.ExistsByUsernameAsync(request.Username))
        {
            return Result<UserResponse>.Failure($"Username '{request.Username}' is already taken.");
        }

        if (await userRepository.ExistsByEmailAsync(request.Email))
        {
            return Result<UserResponse>.Failure($"Email '{request.Email}' is already registered.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            EmployeeId = request.EmployeeId,
            IsActive = true,
            CreatedBy = currentUserService.UserId
        };

        var id = await userRepository.AddAsync(user);

        await auditLogService.LogAsync("Identity_Users", id, "Create", null, JsonSerializer.Serialize(user), currentUserService.UserId);

        return Result<UserResponse>.Success(ToResponse(user));
    }

    public async Task<Result<UserResponse>> UpdateAsync(long id, UpdateUserRequest request)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return Result<UserResponse>.Failure("User not found.");
        }

        if (!request.IsActive && await IsSelfSuperAdminAsync(id))
        {
            return Result<UserResponse>.Failure("A Super Admin cannot deactivate their own account.");
        }

        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase)
            && await userRepository.ExistsByEmailAsync(request.Email, id))
        {
            return Result<UserResponse>.Failure($"Email '{request.Email}' is already registered to another user.");
        }

        var oldValues = JsonSerializer.Serialize(user);

        user.Email = request.Email;
        user.FullName = request.FullName;
        user.IsActive = request.IsActive;
        user.PhoneNumber = request.PhoneNumber;
        user.EmployeeId = request.EmployeeId;
        user.ProfilePictureUrl = request.ProfilePictureUrl;
        user.UpdatedBy = currentUserService.UserId;
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateAsync(user);

        await auditLogService.LogAsync("Identity_Users", id, "Update", oldValues, JsonSerializer.Serialize(user), currentUserService.UserId);

        return Result<UserResponse>.Success(ToResponse(user));
    }

    public async Task<Result<bool>> DeactivateAsync(long id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return Result<bool>.Failure("User not found.");
        }

        if (await IsSelfSuperAdminAsync(id))
        {
            return Result<bool>.Failure("A Super Admin cannot deactivate their own account.");
        }

        // Deactivate only flips IsActive (not IsDeleted) so the user record stays reachable via
        // Detail/Audit Log afterwards — this is not a data-erasing delete, just an account disable.
        var oldValues = JsonSerializer.Serialize(user);
        user.IsActive = false;
        user.UpdatedBy = currentUserService.UserId;
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateAsync(user);
        await auditLogService.LogAsync("Identity_Users", id, "Deactivate", oldValues, JsonSerializer.Serialize(user), currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    // Guards against a Super Admin locking themselves out of the system by deactivating their own account.
    private async Task<bool> IsSelfSuperAdminAsync(long targetUserId)
    {
        if (currentUserService.UserId is null || currentUserService.UserId != targetUserId)
        {
            return false;
        }

        var roleIds = await userRoleRepository.GetRoleIdsByUserIdAsync(targetUserId);
        if (roleIds.Count == 0)
        {
            return false;
        }

        var roles = await roleRepository.GetByIdsAsync(roleIds);
        return roles.Any(r => r.Name == "SuperAdmin");
    }

    public async Task<Result<bool>> ChangePasswordAsync(long id, ChangePasswordRequest request)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return Result<bool>.Failure("User not found.");
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.UpdatedBy = currentUserService.UserId;
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateAsync(user);
        // Old/new values omitted from the audit snapshot - PasswordHash must never appear in logs even hashed.
        await auditLogService.LogAsync("Identity_Users", id, "ChangePassword", null, null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> UnlockAsync(long id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return Result<bool>.Failure("User not found.");
        }

        if (user.LockoutEnd is null && user.FailedLoginAttempts == 0)
        {
            return Result<bool>.Failure("User is not locked out.");
        }

        user.LockoutEnd = null;
        user.FailedLoginAttempts = 0;
        user.UpdatedBy = currentUserService.UserId;
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateAsync(user);
        await auditLogService.LogAsync("Identity_Users", id, "Unlock", null, null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> AssignRoleAsync(long userId, long roleId)
    {
        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result<bool>.Failure("User not found.");
        }

        if (await roleRepository.GetByIdAsync(roleId) is null)
        {
            return Result<bool>.Failure("Role not found.");
        }

        if (await userRoleRepository.ExistsAsync(userId, roleId))
        {
            return Result<bool>.Failure("Role is already assigned to this user.");
        }

        var deletedId = await userRoleRepository.GetDeletedIdAsync(userId, roleId);
        if (deletedId is not null)
        {
            await userRoleRepository.ReactivateAsync(deletedId.Value, currentUserService.UserId ?? 0);
            await auditLogService.LogAsync("Identity_UserRoles", userId, "AssignRole", null, JsonSerializer.Serialize(new { UserId = userId, RoleId = roleId }), currentUserService.UserId);
            return Result<bool>.Success(true);
        }

        var userRole = new UserRole { UserId = userId, RoleId = roleId, CreatedBy = currentUserService.UserId };
        await userRoleRepository.AddAsync(userRole);
        await auditLogService.LogAsync("Identity_UserRoles", userId, "AssignRole", null, JsonSerializer.Serialize(userRole), currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> RevokeRoleAsync(long userId, long roleId)
    {
        if (!await userRoleRepository.ExistsAsync(userId, roleId))
        {
            return Result<bool>.Failure("Role is not assigned to this user.");
        }

        var roleIds = await userRoleRepository.GetRoleIdsByUserIdAsync(userId);
        if (roleIds.Count <= 1)
        {
            return Result<bool>.Failure("A user must have at least one role assigned.");
        }

        await userRoleRepository.RevokeAsync(userId, roleId, currentUserService.UserId ?? 0);
        await auditLogService.LogAsync("Identity_UserRoles", userId, "RevokeRole", JsonSerializer.Serialize(new { UserId = userId, RoleId = roleId }), null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> SetPermissionOverrideAsync(long userId, long permissionId, bool isAllowed)
    {
        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result<bool>.Failure("User not found.");
        }

        if (await permissionRepository.GetByIdAsync(permissionId) is null)
        {
            return Result<bool>.Failure("Permission not found.");
        }

        var existingOverride = await userPermissionRepository.GetOverrideAsync(userId, permissionId);
        if (existingOverride is null)
        {
            await userPermissionRepository.AddAsync(new UserPermission { UserId = userId, PermissionId = permissionId, IsAllowed = isAllowed, CreatedBy = currentUserService.UserId });
        }
        else
        {
            await userPermissionRepository.UpdateIsAllowedAsync(existingOverride.Id, isAllowed, currentUserService.UserId ?? 0);
        }

        await auditLogService.LogAsync("Identity_UserPermissions", userId, "SetPermissionOverride", null, JsonSerializer.Serialize(new { UserId = userId, PermissionId = permissionId, IsAllowed = isAllowed }), currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> RemovePermissionOverrideAsync(long userId, long permissionId)
    {
        var existingOverride = await userPermissionRepository.GetOverrideAsync(userId, permissionId);
        if (existingOverride is null)
        {
            return Result<bool>.Failure("Permission override not found.");
        }

        await userPermissionRepository.RemoveAsync(userId, permissionId, currentUserService.UserId ?? 0);
        await auditLogService.LogAsync("Identity_UserPermissions", userId, "RemovePermissionOverride", JsonSerializer.Serialize(new { UserId = userId, PermissionId = permissionId }), null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    public async Task<UserRolesPermissionsResponse> GetRolesAndPermissionsAsync(long userId)
    {
        var roleIds = await userRoleRepository.GetRoleIdsByUserIdAsync(userId);
        var roles = await roleRepository.GetByIdsAsync(roleIds);
        var effectiveCodes = await permissionResolverService.ResolveAsync(userId);
        var overrides = await userPermissionRepository.GetOverridesByUserIdAsync(userId);
        var allPermissions = await permissionRepository.GetAllAsync();
        var permissionLookup = allPermissions.ToDictionary(p => p.Code);

        var allowOverrideCodes = overrides.Where(o => o.IsAllowed).Select(o => o.Code).ToHashSet();
        var denyOverrideCodes = overrides.Where(o => !o.IsAllowed).Select(o => o.Code).ToHashSet();
        var modifiedAtByCode = overrides.ToDictionary(o => o.Code, o => o.UpdatedAt ?? o.CreatedAt);

        EffectivePermissionDto ToDto(string code, string source) =>
            permissionLookup.TryGetValue(code, out var permission)
                ? new EffectivePermissionDto(permission.Code, permission.Module, permission.Description, source, modifiedAtByCode.GetValueOrDefault(code))
                : new EffectivePermissionDto(code, null, null, source, modifiedAtByCode.GetValueOrDefault(code));

        var effective = effectiveCodes
            .Select(code => ToDto(code, allowOverrideCodes.Contains(code) ? "Allow Override" : "Role"))
            .OrderBy(p => p.Module).ThenBy(p => p.Code)
            .ToList();

        var denied = denyOverrideCodes
            .Select(code => ToDto(code, "Deny Override"))
            .OrderBy(p => p.Module).ThenBy(p => p.Code)
            .ToList();

        return new UserRolesPermissionsResponse(
            roles.Select(r => new UserRoleSummaryDto(r.Id, r.Name)).ToList(),
            effective,
            denied);
    }

    public async Task<PagedResult<UserSessionDto>> GetSessionsAsync(long userId, int page = 1, int pageSize = 10)
    {
        var tokens = await refreshTokenRepository.GetByUserIdAsync(userId);
        var ordered = tokens
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new UserSessionDto(t.Id, t.DeviceInfo, t.IpAddress, t.CreatedAt, t.ExpiresAt, t.RevokedAt))
            .ToList();

        return new PagedResult<UserSessionDto>
        {
            Items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = ordered.Count
        };
    }

    public async Task<Result<bool>> RevokeSessionAsync(long userId, long sessionId)
    {
        var token = await refreshTokenRepository.GetByIdAsync(sessionId);
        if (token is null || token.UserId != userId)
        {
            return Result<bool>.Failure("Session not found.");
        }

        if (token.RevokedAt is not null)
        {
            return Result<bool>.Failure("Session is already revoked.");
        }

        await refreshTokenRepository.RevokeAsync(sessionId);
        await auditLogService.LogAsync("Identity_UserRefreshTokens", sessionId, "ForceLogout", null, null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.Username, user.Email, user.FullName, user.IsActive, user.PhoneNumber, user.EmployeeId, user.ProfilePictureUrl, user.LastLoginAt, user.CreatedAt, user.EmailVerifiedAt, user.PhoneVerifiedAt);
}
