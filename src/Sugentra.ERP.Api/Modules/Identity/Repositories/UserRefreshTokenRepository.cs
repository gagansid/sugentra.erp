using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Queries;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Identity.Repositories;

public interface IUserRefreshTokenRepository
{
    Task<UserRefreshToken?> GetByIdAsync(long id);
    Task<UserRefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task<IReadOnlyList<UserRefreshToken>> GetByUserIdAsync(long userId);
    Task<long> AddAsync(UserRefreshToken entity);
    Task RevokeAsync(long id);
}

public class UserRefreshTokenRepository(IDbConnectionFactory connectionFactory)
    : Repository<UserRefreshToken>(connectionFactory), IUserRefreshTokenRepository
{
    public async Task<UserRefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<UserRefreshToken>(UserRefreshTokenQuery.GetByTokenHashSql, new { TokenHash = tokenHash });
    }

    public async Task<IReadOnlyList<UserRefreshToken>> GetByUserIdAsync(long userId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryListAsync<UserRefreshToken>(UserRefreshTokenQuery.GetByUserIdSql, new { UserId = userId });
    }

    public async Task RevokeAsync(long id)
    {
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(UserRefreshTokenQuery.RevokeSql, new { Id = id });
    }
}
