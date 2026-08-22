namespace Sugentra.ERP.Api.Modules.MasterData.Dtos;

public record BusinessPartnerAddressRequest(string AddressType, string Address, string? City, string? Province, string? PostalCode, string? Country, bool IsPrimary);

public record BusinessPartnerContactRequest(string ContactType, string? ContactName, string Value, bool IsPrimary);

public record CreateBusinessPartnerRequest(
    string Code, string Name, string PartnerType, bool IsActive,
    string? TaxId, string? TaxRegisteredName, string? TaxAddress, string? Nik, string? TaxpayerType,
    string? Nitku, bool IsPkp, string? SktNumber, string? KluCode, string? LogoUrl,
    List<BusinessPartnerAddressRequest> Addresses, List<BusinessPartnerContactRequest> Contacts);

// Update replaces the full address/contact sets — same simplest-correct pattern as BillOfMaterialUseCase.
public record UpdateBusinessPartnerRequest(
    string Name, string PartnerType, bool IsActive,
    string? TaxId, string? TaxRegisteredName, string? TaxAddress, string? Nik, string? TaxpayerType,
    string? Nitku, bool IsPkp, string? SktNumber, string? KluCode, string? LogoUrl,
    List<BusinessPartnerAddressRequest> Addresses, List<BusinessPartnerContactRequest> Contacts);

public record BusinessPartnerAddressResponse(long Id, string AddressType, string Address, string? City, string? Province, string? PostalCode, string? Country, bool IsPrimary);

public record BusinessPartnerContactResponse(long Id, string ContactType, string? ContactName, string Value, bool IsPrimary);

public record BusinessPartnerResponse(
    long Id, string Code, string Name, string PartnerType, bool IsActive,
    string? TaxId, string? TaxRegisteredName, string? TaxAddress, string? Nik, string? TaxpayerType,
    string? Nitku, bool IsPkp, string? SktNumber, string? KluCode, string? LogoUrl, DateTime CreatedAt,
    IReadOnlyList<BusinessPartnerAddressResponse> Addresses, IReadOnlyList<BusinessPartnerContactResponse> Contacts);
