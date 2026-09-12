namespace Sugentra.ERP.Api.Shared.Contracts;

/// <summary>Cross-module contract for looking up BusinessPartner master-data fields needed by other modules
/// (e.g. Procurement needs a Vendor's Code/Name for PO responses without querying MasterData's tables directly).
/// Implemented by MasterData — callers never reference MasterData's BusinessPartner entity/repository directly.</summary>
public record BusinessPartnerSummary(long Id, string Code, string Name, string PartnerType);

public interface IBusinessPartnerDirectoryService
{
    Task<BusinessPartnerSummary?> GetSummaryAsync(long businessPartnerId);
}
