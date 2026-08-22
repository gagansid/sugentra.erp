using System.Text.Json;
using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Logging;

namespace Sugentra.ERP.Api.Modules.Identity.UseCases;

public class RoleUseCase(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IRolePermissionRepository rolePermissionRepository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    // Read path: Controller talks only to UseCase, which delegates to Repository internally (keeps the Controller uniform).
    public Task<PagedResult<RoleListItemDto>> GetPagedAsync(RoleListRequest request) =>
        roleRepository.GetPagedAsync(request);

    public async Task<RoleResponse?> GetByIdAsync(long id)
    {
        var role = await roleRepository.GetByIdAsync(id);
        return role is null ? null : ToResponse(role);
    }

    public async Task<Result<RoleResponse>> CreateAsync(CreateRoleRequest request)
    {
        if (await roleRepository.ExistsByNameAsync(request.Name))
        {
            return Result<RoleResponse>.Failure($"Role '{request.Name}' already exists.");
        }

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedBy = currentUserService.UserId
        };

        var id = await roleRepository.AddAsync(role);

        await auditLogService.LogAsync("Identity_Roles", id, "Create", null, JsonSerializer.Serialize(role), currentUserService.UserId);

        return Result<RoleResponse>.Success(ToResponse(role));
    }

    public async Task<Result<RoleResponse>> UpdateAsync(long id, UpdateRoleRequest request)
    {
        var role = await roleRepository.GetByIdAsync(id);
        if (role is null)
        {
            return Result<RoleResponse>.Failure("Role not found.");
        }

        var oldValues = JsonSerializer.Serialize(role);

        role.Name = request.Name;
        role.Description = request.Description;
        role.IsActive = request.IsActive;
        role.UpdatedBy = currentUserService.UserId;
        role.UpdatedAt = DateTime.UtcNow;

        await roleRepository.UpdateAsync(role);

        await auditLogService.LogAsync("Identity_Roles", id, "Update", oldValues, JsonSerializer.Serialize(role), currentUserService.UserId);

        return Result<RoleResponse>.Success(ToResponse(role));
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var role = await roleRepository.GetByIdAsync(id);
        if (role is null)
        {
            return Result<bool>.Failure("Role not found.");
        }

        await roleRepository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);
        await auditLogService.LogAsync("Identity_Roles", id, "SoftDelete", JsonSerializer.Serialize(role), null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> AssignPermissionAsync(long roleId, long permissionId)
    {
        if (await roleRepository.GetByIdAsync(roleId) is null)
        {
            return Result<bool>.Failure("Role not found.");
        }

        if (await permissionRepository.GetByIdAsync(permissionId) is null)
        {
            return Result<bool>.Failure("Permission not found.");
        }

        if (await rolePermissionRepository.ExistsAsync(roleId, permissionId))
        {
            return Result<bool>.Failure("Permission is already assigned to this role.");
        }

        var inactiveId = await rolePermissionRepository.GetInactiveIdAsync(roleId, permissionId);
        if (inactiveId is not null)
        {
            await rolePermissionRepository.ActivateAsync(inactiveId.Value, currentUserService.UserId ?? 0);
            await auditLogService.LogAsync("Identity_RolePermissions", roleId, "AssignPermission", null, JsonSerializer.Serialize(new { RoleId = roleId, PermissionId = permissionId }), currentUserService.UserId);
            return Result<bool>.Success(true);
        }

        var rolePermission = new RolePermission { RoleId = roleId, PermissionId = permissionId, CreatedBy = currentUserService.UserId };
        await rolePermissionRepository.AddAsync(rolePermission);
        await auditLogService.LogAsync("Identity_RolePermissions", roleId, "AssignPermission", null, JsonSerializer.Serialize(rolePermission), currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> RevokePermissionAsync(long roleId, long permissionId)
    {
        if (!await rolePermissionRepository.ExistsAsync(roleId, permissionId))
        {
            return Result<bool>.Failure("Permission is not assigned to this role.");
        }

        await rolePermissionRepository.RevokeAsync(roleId, permissionId, currentUserService.UserId ?? 0);
        await auditLogService.LogAsync("Identity_RolePermissions", roleId, "RevokePermission", JsonSerializer.Serialize(new { RoleId = roleId, PermissionId = permissionId }), null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<PagedResult<RolePermissionListItemDto>>> GetPermissionsAsync(long roleId, RolePermissionListRequest request)
    {
        if (await roleRepository.GetByIdAsync(roleId) is null)
        {
            return Result<PagedResult<RolePermissionListItemDto>>.Failure("Role not found.");
        }

        return Result<PagedResult<RolePermissionListItemDto>>.Success(await rolePermissionRepository.GetPagedForRoleAsync(roleId, request));
    }

    private static RoleResponse ToResponse(Role role) =>
        new(role.Id, role.Name, role.Description, role.IsActive, role.CreatedAt);
}
