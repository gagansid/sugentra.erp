using Sugentra.ERP.Api.Modules.MasterData.Entities;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData.Services;

public class BusinessPartnerDirectoryService(GenericRepository<BusinessPartner> businessPartnerRepository)
    : IBusinessPartnerDirectoryService
{
    public async Task<BusinessPartnerSummary?> GetSummaryAsync(long businessPartnerId)
    {
        var partner = await businessPartnerRepository.GetByIdAsync(businessPartnerId);
        return partner is null ? null : new BusinessPartnerSummary(partner.Id, partner.Code, partner.Name, partner.PartnerType);
    }
}
