using Sugentra.ERP.Api.Modules.MasterData.Entities;
using Sugentra.ERP.Api.Modules.MasterData.Queries;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData.Repositories;

public interface IBusinessPartnerContactRepository
{
    Task<IReadOnlyList<BusinessPartnerContact>> GetByPartnerIdAsync(long businessPartnerId);
    Task<long> AddAsync(BusinessPartnerContact entity);
    Task SoftDeleteByPartnerIdAsync(long businessPartnerId, long deletedBy);
}

public class BusinessPartnerContactRepository(IDbConnectionFactory connectionFactory)
    : Repository<BusinessPartnerContact>(connectionFactory), IBusinessPartnerContactRepository
{
    public async Task<IReadOnlyList<BusinessPartnerContact>> GetByPartnerIdAsync(long businessPartnerId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryListAsync<BusinessPartnerContact>(BusinessPartnerContactQuery.GetByPartnerIdSql, new { BusinessPartnerId = businessPartnerId });
    }

    public async Task SoftDeleteByPartnerIdAsync(long businessPartnerId, long deletedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(BusinessPartnerContactQuery.SoftDeleteByPartnerIdSql, new { BusinessPartnerId = businessPartnerId, DeletedBy = deletedBy });
    }
}
