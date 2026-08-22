using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Queries;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Identity.Repositories;

public interface IRolePermissionRepository
{
    Task<bool> ExistsAsync(long roleId, long permissionId);
    Task<IReadOnlyList<string>> GetPermissionCodesByRoleIdsAsync(IEnumerable<long> roleIds);
    Task<long?> GetInactiveIdAsync(long roleId, long permissionId);
    Task ActivateAsync(long id, long updatedBy);
    Task<long> AddAsync(RolePermission entity);
    Task RevokeAsync(long roleId, long permissionId, long updatedBy);
    Task<PagedResult<RolePermissionListItemDto>> GetPagedForRoleAsync(long roleId, RolePermissionListRequest request);
}

public class RolePermissionRepository(IDbConnectionFactory connectionFactory)
    : Repository<RolePermission>(connectionFactory), IRolePermissionRepository
{
    public async Task<bool> ExistsAsync(long roleId, long permissionId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryScalarAsync<int>(RolePermissionQuery.ExistsSql, new { RoleId = roleId, PermissionId = permissionId }) > 0;
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesByRoleIdsAsync(IEnumerable<long> roleIds)
    {
        var ids = roleIds as IReadOnlyCollection<long> ?? roleIds.ToList();
        if (ids.Count == 0)
        {
            return Array.Empty<string>();
        }

        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryListAsync<string>(RolePermissionQuery.GetPermissionCodesByRoleIdsSql, new { RoleIds = ids });
    }

    public async Task RevokeAsync(long roleId, long permissionId, long updatedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(RolePermissionQuery.RevokeSql, new { RoleId = roleId, PermissionId = permissionId, UpdatedBy = updatedBy });
    }

    public async Task<long?> GetInactiveIdAsync(long roleId, long permissionId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<long?>(RolePermissionQuery.GetInactiveIdSql, new { RoleId = roleId, PermissionId = permissionId });
    }

    public async Task ActivateAsync(long id, long updatedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(RolePermissionQuery.ActivateSql, new { Id = id, UpdatedBy = updatedBy });
    }

    public async Task<PagedResult<RolePermissionListItemDto>> GetPagedForRoleAsync(long roleId, RolePermissionListRequest request)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var rows = await connection.QueryListAsync<RolePermissionPagedRow>(RolePermissionQuery.GetPagedForRoleSql, new
        {
            RoleId = roleId,
            request.Module,
            request.Code,
            request.Description,
            request.IsAssigned,
            request.Page,
            request.PageSize
        });

        return new PagedResult<RolePermissionListItemDto>
        {
            Items = rows.Select(r => new RolePermissionListItemDto(r.Id, r.Code, r.Module, r.Description, r.IsAssigned, r.ModifiedAt)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = rows.Count > 0 ? rows[0].TotalCount : 0
        };
    }

    private record RolePermissionPagedRow(long Id, string Code, string? Module, string? Description, bool IsAssigned, DateTime? ModifiedAt, int TotalCount);
}
