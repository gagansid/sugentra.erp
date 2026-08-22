using Sugentra.ERP.Api.Modules.MasterData.Entities;
using Sugentra.ERP.Api.Modules.MasterData.Queries;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData.Repositories;

public interface IBusinessPartnerAddressRepository
{
    Task<IReadOnlyList<BusinessPartnerAddress>> GetByPartnerIdAsync(long businessPartnerId);
    Task<long> AddAsync(BusinessPartnerAddress entity);
    Task SoftDeleteByPartnerIdAsync(long businessPartnerId, long deletedBy);
}

public class BusinessPartnerAddressRepository(IDbConnectionFactory connectionFactory)
    : Repository<BusinessPartnerAddress>(connectionFactory), IBusinessPartnerAddressRepository
{
    public async Task<IReadOnlyList<BusinessPartnerAddress>> GetByPartnerIdAsync(long businessPartnerId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryListAsync<BusinessPartnerAddress>(BusinessPartnerAddressQuery.GetByPartnerIdSql, new { BusinessPartnerId = businessPartnerId });
    }

    public async Task SoftDeleteByPartnerIdAsync(long businessPartnerId, long deletedBy)
    {
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(BusinessPartnerAddressQuery.SoftDeleteByPartnerIdSql, new { BusinessPartnerId = businessPartnerId, DeletedBy = deletedBy });
    }
}
