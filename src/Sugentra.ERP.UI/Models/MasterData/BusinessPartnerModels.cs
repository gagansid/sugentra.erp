namespace Sugentra.ERP.UI.Models.MasterData;

public record BusinessPartnerAddressRequest(string AddressType, string Address, string? City, string? Province, string? PostalCode, string? Country, bool IsPrimary);

public record BusinessPartnerContactRequest(string ContactType, string? ContactName, string Value, bool IsPrimary);

public record CreateBusinessPartnerRequest(
    string Code, string Name, string PartnerType, bool IsActive,
    string? TaxId, string? TaxRegisteredName, string? TaxAddress, string? Nik, string? TaxpayerType,
    string? Nitku, bool IsPkp, string? SktNumber, string? KluCode, string? LogoUrl,
    List<BusinessPartnerAddressRequest> Addresses, List<BusinessPartnerContactRequest> Contacts);

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

public record BusinessPartnerListItemDto(long Id, string Code, string Name, string PartnerType, string? TaxId, bool IsActive);

public record BusinessPartnerListRequest(string? Keyword = null, int Page = 1, int PageSize = 10, bool? IsActive = null);

public record BusinessPartnerAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);
