using System.Text.Json;
using Sugentra.ERP.Api.Modules.MasterData.Dtos;
using Sugentra.ERP.Api.Modules.MasterData.Entities;
using Sugentra.ERP.Api.Modules.MasterData.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Logging;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData.UseCases;

public class BusinessPartnerUseCase(
    GenericRepository<BusinessPartner> partnerRepository,
    IBusinessPartnerAddressRepository addressRepository,
    IBusinessPartnerContactRepository contactRepository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    public async Task<IReadOnlyList<BusinessPartner>> GetAllAsync() => await partnerRepository.GetAllAsync();

    public async Task<BusinessPartnerResponse?> GetByIdAsync(long id)
    {
        var partner = await partnerRepository.GetByIdAsync(id);
        return partner is null ? null : await ToResponseAsync(partner);
    }

    public async Task<Result<BusinessPartnerResponse>> CreateAsync(CreateBusinessPartnerRequest request)
    {
        var validation = ValidateTaxFields(request.TaxpayerType, request.TaxId, request.TaxRegisteredName, request.TaxAddress, request.Nik)
            ?? ValidateOnePrimaryPerType(request.Addresses, request.Contacts);
        if (validation is not null)
        {
            return Result<BusinessPartnerResponse>.Failure(validation);
        }

        var partner = new BusinessPartner
        {
            Code = request.Code,
            Name = request.Name,
            PartnerType = request.PartnerType,
            IsActive = request.IsActive,
            TaxId = request.TaxId,
            TaxRegisteredName = request.TaxRegisteredName,
            TaxAddress = request.TaxAddress,
            Nik = request.Nik,
            TaxpayerType = request.TaxpayerType,
            Nitku = request.Nitku,
            IsPkp = request.IsPkp,
            SktNumber = request.SktNumber,
            KluCode = request.KluCode,
            LogoUrl = request.LogoUrl,
            CreatedBy = currentUserService.UserId
        };

        var id = await partnerRepository.AddAsync(partner);
        await AddChildrenAsync(id, request.Addresses, request.Contacts);

        var response = await ToResponseAsync(partner);
        await auditLogService.LogAsync("MasterData_BusinessPartners", id, "Create", null, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<BusinessPartnerResponse>.Success(response);
    }

    public async Task<Result<BusinessPartnerResponse>> UpdateAsync(long id, UpdateBusinessPartnerRequest request)
    {
        var partner = await partnerRepository.GetByIdAsync(id);
        if (partner is null)
        {
            return Result<BusinessPartnerResponse>.Failure($"Business partner {id} not found.");
        }

        var validation = ValidateTaxFields(request.TaxpayerType, request.TaxId, request.TaxRegisteredName, request.TaxAddress, request.Nik)
            ?? ValidateOnePrimaryPerType(request.Addresses, request.Contacts);
        if (validation is not null)
        {
            return Result<BusinessPartnerResponse>.Failure(validation);
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(partner));

        partner.Name = request.Name;
        partner.PartnerType = request.PartnerType;
        partner.IsActive = request.IsActive;
        partner.TaxId = request.TaxId;
        partner.TaxRegisteredName = request.TaxRegisteredName;
        partner.TaxAddress = request.TaxAddress;
        partner.Nik = request.Nik;
        partner.TaxpayerType = request.TaxpayerType;
        partner.Nitku = request.Nitku;
        partner.IsPkp = request.IsPkp;
        partner.SktNumber = request.SktNumber;
        partner.KluCode = request.KluCode;
        partner.LogoUrl = request.LogoUrl;
        partner.UpdatedBy = currentUserService.UserId;
        partner.UpdatedAt = DateTime.UtcNow;

        await partnerRepository.UpdateAsync(partner);

        // Full address/contact-set replace — simplest correct behavior, same pattern as BillOfMaterialUseCase.
        await addressRepository.SoftDeleteByPartnerIdAsync(id, currentUserService.UserId ?? 0);
        await contactRepository.SoftDeleteByPartnerIdAsync(id, currentUserService.UserId ?? 0);
        await AddChildrenAsync(id, request.Addresses, request.Contacts);

        var response = await ToResponseAsync(partner);
        await auditLogService.LogAsync("MasterData_BusinessPartners", id, "Update", oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<BusinessPartnerResponse>.Success(response);
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var partner = await partnerRepository.GetByIdAsync(id);
        if (partner is null)
        {
            return Result<bool>.Failure($"Business partner {id} not found.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(partner));

        await addressRepository.SoftDeleteByPartnerIdAsync(id, currentUserService.UserId ?? 0);
        await contactRepository.SoftDeleteByPartnerIdAsync(id, currentUserService.UserId ?? 0);
        await partnerRepository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);

        await auditLogService.LogAsync("MasterData_BusinessPartners", id, "SoftDelete", oldValues, null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    private static string? ValidateTaxFields(string? taxpayerType, string? taxId, string? taxRegisteredName, string? taxAddress, string? nik)
    {
        if (taxpayerType == "Badan" && (string.IsNullOrWhiteSpace(taxId) || string.IsNullOrWhiteSpace(taxRegisteredName) || string.IsNullOrWhiteSpace(taxAddress)))
        {
            return "TaxId, TaxRegisteredName, and TaxAddress are required when TaxpayerType is Badan.";
        }

        if (taxpayerType == "OrangPribadi" && string.IsNullOrWhiteSpace(nik))
        {
            return "Nik is required when TaxpayerType is OrangPribadi.";
        }

        return null;
    }

    // Mirrors the DB-level filtered unique indexes (0017) so violations surface as a 422 instead of a raw SQL exception.
    private static string? ValidateOnePrimaryPerType(List<BusinessPartnerAddressRequest> addresses, List<BusinessPartnerContactRequest> contacts)
    {
        var duplicateAddressType = addresses
            .Where(a => a.IsPrimary)
            .GroupBy(a => a.AddressType)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicateAddressType is not null)
        {
            return $"Only one primary address is allowed per address type ('{duplicateAddressType.Key}').";
        }

        var duplicateContactType = contacts
            .Where(c => c.IsPrimary)
            .GroupBy(c => c.ContactType)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicateContactType is not null)
        {
            return $"Only one primary contact is allowed per contact type ('{duplicateContactType.Key}').";
        }

        return null;
    }

    private async Task AddChildrenAsync(long partnerId, List<BusinessPartnerAddressRequest> addresses, List<BusinessPartnerContactRequest> contacts)
    {
        foreach (var address in addresses)
        {
            await addressRepository.AddAsync(new BusinessPartnerAddress
            {
                BusinessPartnerId = partnerId,
                AddressType = address.AddressType,
                Address = address.Address,
                City = address.City,
                Province = address.Province,
                PostalCode = address.PostalCode,
                Country = address.Country,
                IsPrimary = address.IsPrimary,
                CreatedBy = currentUserService.UserId
            });
        }

        foreach (var contact in contacts)
        {
            await contactRepository.AddAsync(new BusinessPartnerContact
            {
                BusinessPartnerId = partnerId,
                ContactType = contact.ContactType,
                ContactName = contact.ContactName,
                Value = contact.Value,
                IsPrimary = contact.IsPrimary,
                CreatedBy = currentUserService.UserId
            });
        }
    }

    private async Task<BusinessPartnerResponse> ToResponseAsync(BusinessPartner partner)
    {
        var addresses = await addressRepository.GetByPartnerIdAsync(partner.Id);
        var contacts = await contactRepository.GetByPartnerIdAsync(partner.Id);

        var addressResponses = addresses.Select(a => new BusinessPartnerAddressResponse(a.Id, a.AddressType, a.Address, a.City, a.Province, a.PostalCode, a.Country, a.IsPrimary)).ToList();
        var contactResponses = contacts.Select(c => new BusinessPartnerContactResponse(c.Id, c.ContactType, c.ContactName, c.Value, c.IsPrimary)).ToList();

        return new BusinessPartnerResponse(
            partner.Id, partner.Code, partner.Name, partner.PartnerType, partner.IsActive,
            partner.TaxId, partner.TaxRegisteredName, partner.TaxAddress, partner.Nik, partner.TaxpayerType,
            partner.Nitku, partner.IsPkp, partner.SktNumber, partner.KluCode, partner.LogoUrl, partner.CreatedAt,
            addressResponses, contactResponses);
    }
}
