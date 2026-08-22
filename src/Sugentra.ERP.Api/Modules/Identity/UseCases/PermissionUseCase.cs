using System.Text.Json;
using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Logging;

namespace Sugentra.ERP.Api.Modules.Identity.UseCases;

public class PermissionUseCase(
    IPermissionRepository permissionRepository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    // Read path: Controller talks only to UseCase, which delegates to Repository internally (keeps the Controller uniform).
    public Task<PagedResult<PermissionListItemDto>> GetPagedAsync(PermissionListRequest request) =>
        permissionRepository.GetPagedAsync(request);

    public async Task<PermissionResponse?> GetByIdAsync(long id)
    {
        var permission = await permissionRepository.GetByIdAsync(id);
        return permission is null ? null : ToResponse(permission);
    }

    public async Task<Result<PermissionResponse>> CreateAsync(CreatePermissionRequest request)
    {
        if (await permissionRepository.ExistsByCodeAsync(request.Code))
        {
            return Result<PermissionResponse>.Failure($"Permission code '{request.Code}' already exists.");
        }

        var permission = new Permission
        {
            Code = request.Code,
            Module = request.Module,
            Description = request.Description,
            CreatedBy = currentUserService.UserId
        };

        var id = await permissionRepository.AddAsync(permission);

        await auditLogService.LogAsync("Identity_Permissions", id, "Create", null, JsonSerializer.Serialize(permission), currentUserService.UserId);

        return Result<PermissionResponse>.Success(ToResponse(permission));
    }

    public async Task<Result<PermissionResponse>> UpdateAsync(long id, UpdatePermissionRequest request)
    {
        var permission = await permissionRepository.GetByIdAsync(id);
        if (permission is null)
        {
            return Result<PermissionResponse>.Failure("Permission not found.");
        }

        var oldValues = JsonSerializer.Serialize(permission);

        permission.Module = request.Module;
        permission.Description = request.Description;
        permission.UpdatedBy = currentUserService.UserId;
        permission.UpdatedAt = DateTime.UtcNow;

        await permissionRepository.UpdateAsync(permission);

        await auditLogService.LogAsync("Identity_Permissions", id, "Update", oldValues, JsonSerializer.Serialize(permission), currentUserService.UserId);

        return Result<PermissionResponse>.Success(ToResponse(permission));
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var permission = await permissionRepository.GetByIdAsync(id);
        if (permission is null)
        {
            return Result<bool>.Failure("Permission not found.");
        }

        await permissionRepository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);
        await auditLogService.LogAsync("Identity_Permissions", id, "SoftDelete", JsonSerializer.Serialize(permission), null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    private static PermissionResponse ToResponse(Permission permission) =>
        new(permission.Id, permission.Code, permission.Module, permission.Description, permission.CreatedAt);
}
