using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Modules.Settings.Queries;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Settings.Repositories;

public interface ISystemParameterRepository
{
    Task<SystemParameter?> GetByIdAsync(long id);
    Task<IReadOnlyList<SystemParameter>> GetAllAsync();
    Task<long> AddAsync(SystemParameter entity);
    Task UpdateAsync(SystemParameter entity);
    Task SoftDeleteAsync(long id, long deletedBy);
    Task<string?> GetValueAsync(string? category, string key);
}

public class SystemParameterRepository(IDbConnectionFactory connectionFactory)
    : GenericRepository<SystemParameter>(connectionFactory), ISystemParameterRepository
{
    public async Task<string?> GetValueAsync(string? category, string key)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var row = await connection.QuerySingleStoredProcedureAsync<SystemParameterRow?>(
            SystemParameterQuery.GetByKeyProcedureName, new { ParamCategory = category, ParamKey = key });
        return row?.ParamValue;
    }
}
