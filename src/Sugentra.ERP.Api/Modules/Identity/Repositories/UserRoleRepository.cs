using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Queries;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Identity.Repositories;

public interface IUserRoleRepository
{
    Task<bool> ExistsAsync(long userId, long roleId);
    Task<IReadOnlyList<long>> GetRoleIdsByUserIdAsync(long userId);
    Task<IReadOnlyList<long>> GetUserIdsByRoleIdAsync(long roleId);
    Task<long?> GetDeletedIdAsync(long userId, long roleId);
    Task ReactivateAsync(long id, long createdBy);
    Task<long> AddAsync(UserRole entity);
    Task RevokeAsync(long userId, long roleId, long deletedBy);
}

public class UserRoleRepository(IDbConnectionFactory connectionFactory)
    : Repository<UserRole>(connectionFactory), IUserRoleRepository
{
    public async Task<bool> ExistsAsync(long userId, long roleId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryScalarAsync<int>(UserRoleQuery.ExistsSql, new { UserId = userId, RoleId = roleId }) > 0;
    }

    public async Task<IReadOnlyList<long>> GetRoleIdsByUserIdAsync(long userId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryListAsync<long>(UserRoleQuery.GetRoleIdsByUserIdSql, new { UserId = userId });
    }

    public async Task<IReadOnlyList<long>> GetUserIdsByRoleIdAsync(long roleId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryListAsync<long>(UserRoleQuery.GetUserIdsByRoleIdSql, new { RoleId = roleId });
    }

    public async Task<long?> GetDeletedIdAsync(long userId, long roleId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<long?>(UserRoleQuery.GetDeletedIdSql, new { UserId = userId, RoleId = roleId });
    }

    public async Task ReactivateAsync(long id, long createdBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(UserRoleQuery.ReactivateSql, new { Id = id, CreatedBy = createdBy });
    }

    public async Task RevokeAsync(long userId, long roleId, long deletedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(UserRoleQuery.RevokeSql, new { UserId = userId, RoleId = roleId, DeletedBy = deletedBy });
    }
}
