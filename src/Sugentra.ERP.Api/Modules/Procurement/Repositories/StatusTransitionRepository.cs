using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Procurement.Repositories;

public interface IStatusTransitionRepository
{
    Task<bool> IsAllowedAsync(string categoryGroup, string fromStatus, string toStatus);
}

// Looks up Procurement_StatusTransitions so the allowed-transition graph for LifecycleStatus stays
// data-driven instead of hardcoded in each UseCase - see docs/modules/procurement.md.
public class StatusTransitionRepository(IDbConnectionFactory connectionFactory) : IStatusTransitionRepository
{
    public async Task<bool> IsAllowedAsync(string categoryGroup, string fromStatus, string toStatus)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
            SELECT COUNT(1) FROM Procurement_StatusTransitions
            WHERE CategoryGroup = @CategoryGroup AND FromStatus = @FromStatus AND ToStatus = @ToStatus
            """;
        var count = await connection.QueryScalarAsync<int>(sql, new { CategoryGroup = categoryGroup, FromStatus = fromStatus, ToStatus = toStatus });
        return count > 0;
    }
}
