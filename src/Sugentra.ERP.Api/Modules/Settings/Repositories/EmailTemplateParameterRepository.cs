using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Modules.Settings.Queries;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Settings.Repositories;

public interface IEmailTemplateParameterRepository
{
    Task<EmailTemplateParameter?> GetByIdAsync(long id);
    Task<IReadOnlyList<EmailTemplateParameter>> GetAllAsync();
    Task<long> AddAsync(EmailTemplateParameter entity);
    Task UpdateAsync(EmailTemplateParameter entity);
    Task SoftDeleteAsync(long id, long deletedBy);
    Task<IReadOnlyList<EmailTemplateParameterRow>> GetByTemplateCodeAsync(string templateCode);
}

public class EmailTemplateParameterRepository(IDbConnectionFactory connectionFactory)
    : GenericRepository<EmailTemplateParameter>(connectionFactory), IEmailTemplateParameterRepository
{
    public async Task<IReadOnlyList<EmailTemplateParameterRow>> GetByTemplateCodeAsync(string templateCode)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryStoredProcedureAsync<EmailTemplateParameterRow>(
            EmailTemplateParameterQuery.GetByTemplateCodeProcedureName, new { TemplateCode = templateCode });
    }
}
