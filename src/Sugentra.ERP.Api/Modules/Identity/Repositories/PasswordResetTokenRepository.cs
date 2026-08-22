using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Queries;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Identity.Repositories;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetActiveByUserAndCodeAsync(long userId, string code);
    Task<long> AddAsync(PasswordResetToken entity);
    Task MarkUsedAsync(long id);
}

public class PasswordResetTokenRepository(IDbConnectionFactory connectionFactory)
    : Repository<PasswordResetToken>(connectionFactory), IPasswordResetTokenRepository
{
    public async Task<PasswordResetToken?> GetActiveByUserAndCodeAsync(long userId, string code)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<PasswordResetToken>(
            PasswordResetTokenQuery.GetActiveByUserAndCodeSql, new { UserId = userId, Code = code });
    }

    public async Task MarkUsedAsync(long id)
    {
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(PasswordResetTokenQuery.MarkUsedSql, new { Id = id });
    }
}
