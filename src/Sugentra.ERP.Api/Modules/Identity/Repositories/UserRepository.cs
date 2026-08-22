using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Queries;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Identity.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email, long excludeUserId);
    Task<PagedResult<UserListItemDto>> GetPagedAsync(UserListRequest request);
    Task<long> AddAsync(User entity);
    Task UpdateAsync(User entity);
    Task SoftDeleteAsync(long id, long deletedBy);
}

public class UserRepository(IDbConnectionFactory connectionFactory)
    : Repository<User>(connectionFactory), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<User>(UserListQuery.GetByUsernameSql, new { Username = username });
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<User>(UserListQuery.GetByUsernameOrEmailSql, new { UsernameOrEmail = usernameOrEmail });
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryScalarAsync<int>(UserListQuery.ExistsByUsernameSql, new { Username = username }) > 0;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryScalarAsync<int>(UserListQuery.ExistsByEmailSql, new { Email = email }) > 0;
    }

    public async Task<bool> ExistsByEmailAsync(string email, long excludeUserId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryScalarAsync<int>(UserListQuery.ExistsByEmailForOtherUserSql, new { Email = email, ExcludeId = excludeUserId }) > 0;
    }

    public async Task<PagedResult<UserListItemDto>> GetPagedAsync(UserListRequest request)
    {
        using var connection = ConnectionFactory.CreateConnection();
        // request's property names already match the SP's @Parameter names 1:1 — no anonymous object wrapper needed.
        var rows = await connection.QueryStoredProcedureAsync<UserPagedRow>(UserListQuery.GetPagedProcedureName, request);

        return new PagedResult<UserListItemDto>
        {
            Items = rows.Select(r => new UserListItemDto(r.Id, r.Username, r.Email, r.FullName, r.IsActive, r.LockoutEnd, r.IsBanned)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = rows.Count > 0 ? rows[0].TotalCount : 0
        };
    }

    private record UserPagedRow(long Id, string Username, string Email, string FullName, bool IsActive, DateTime? LockoutEnd, bool IsBanned, int TotalCount);
}
