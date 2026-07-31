using Dapper;
using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Identity.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string email);
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
        const string sql = "SELECT * FROM Identity_Users WHERE Username = @Username AND IsDeleted = 0";
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM Identity_Users WHERE Username = @Username AND IsDeleted = 0";
        return await connection.ExecuteScalarAsync<int>(sql, new { Username = username }) > 0;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM Identity_Users WHERE Email = @Email AND IsDeleted = 0";
        return await connection.ExecuteScalarAsync<int>(sql, new { Email = email }) > 0;
    }
}
