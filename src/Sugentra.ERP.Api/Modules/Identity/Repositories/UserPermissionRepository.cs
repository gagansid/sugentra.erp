using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Queries;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Identity.Repositories;

public record PermissionOverride(string Code, bool IsAllowed, DateTime CreatedAt, DateTime? UpdatedAt);

public interface IUserPermissionRepository
{
    Task<UserPermission?> GetOverrideAsync(long userId, long permissionId);
    Task<IReadOnlyList<PermissionOverride>> GetOverridesByUserIdAsync(long userId);
    Task<long> AddAsync(UserPermission entity);
    Task UpdateIsAllowedAsync(long id, bool isAllowed, long updatedBy);
    Task RemoveAsync(long userId, long permissionId, long deletedBy);
}

public class UserPermissionRepository(IDbConnectionFactory connectionFactory)
    : Repository<UserPermission>(connectionFactory), IUserPermissionRepository
{
    public async Task<UserPermission?> GetOverrideAsync(long userId, long permissionId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<UserPermission>(UserPermissionQuery.GetOverrideSql, new { UserId = userId, PermissionId = permissionId });
    }

    public async Task<IReadOnlyList<PermissionOverride>> GetOverridesByUserIdAsync(long userId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryListAsync<PermissionOverride>(UserPermissionQuery.GetOverridesByUserIdSql, new { UserId = userId });
    }

    public async Task UpdateIsAllowedAsync(long id, bool isAllowed, long updatedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(UserPermissionQuery.UpdateIsAllowedSql, new { Id = id, IsAllowed = isAllowed, UpdatedBy = updatedBy });
    }

    public async Task RemoveAsync(long userId, long permissionId, long deletedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(UserPermissionQuery.RemoveSql, new { UserId = userId, PermissionId = permissionId, DeletedBy = deletedBy });
    }
}
