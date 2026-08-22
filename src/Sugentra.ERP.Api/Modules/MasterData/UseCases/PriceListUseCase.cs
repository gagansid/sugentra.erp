using System.Text.Json;
using Sugentra.ERP.Api.Modules.MasterData.Dtos;
using Sugentra.ERP.Api.Modules.MasterData.Entities;
using Sugentra.ERP.Api.Modules.MasterData.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Logging;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData.UseCases;

public class PriceListUseCase(
    GenericRepository<PriceListHeader> headerRepository,
    IPriceListLineRepository lineRepository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    public async Task<IReadOnlyList<PriceListResponse>> GetAllAsync()
    {
        var headers = await headerRepository.GetAllAsync();
        return await Task.WhenAll(headers.Select(ToResponseAsync));
    }

    public async Task<PriceListResponse?> GetByIdAsync(long id)
    {
        var header = await headerRepository.GetByIdAsync(id);
        return header is null ? null : await ToResponseAsync(header);
    }

    public async Task<PriceListResponse> CreateAsync(CreatePriceListRequest request)
    {
        var header = new PriceListHeader
        {
            Name = request.Name,
            Type = request.Type,
            CurrencyId = request.CurrencyId,
            BusinessPartnerId = request.BusinessPartnerId,
            CreatedBy = currentUserService.UserId
        };

        var id = await headerRepository.AddAsync(header);
        await AddLinesAsync(id, request.Lines);

        var response = await ToResponseAsync(header);
        await auditLogService.LogAsync("MasterData_PriceListHeaders", id, "Create", null, JsonSerializer.Serialize(response), currentUserService.UserId);

        return response;
    }

    public async Task<Result<PriceListResponse>> UpdateAsync(long id, UpdatePriceListRequest request)
    {
        var header = await headerRepository.GetByIdAsync(id);
        if (header is null)
        {
            return Result<PriceListResponse>.Failure($"Price list {id} not found.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(header));

        header.Name = request.Name;
        header.Type = request.Type;
        header.CurrencyId = request.CurrencyId;
        header.BusinessPartnerId = request.BusinessPartnerId;
        header.IsActive = request.IsActive;
        header.UpdatedBy = currentUserService.UserId;
        header.UpdatedAt = DateTime.UtcNow;

        await headerRepository.UpdateAsync(header);

        // Full line-set replace — same simplest-correct pattern as BillOfMaterial lines.
        await lineRepository.SoftDeleteByHeaderIdAsync(id, currentUserService.UserId ?? 0);
        await AddLinesAsync(id, request.Lines);

        var response = await ToResponseAsync(header);
        await auditLogService.LogAsync("MasterData_PriceListHeaders", id, "Update", oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<PriceListResponse>.Success(response);
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var header = await headerRepository.GetByIdAsync(id);
        if (header is null)
        {
            return Result<bool>.Failure($"Price list {id} not found.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(header));

        await lineRepository.SoftDeleteByHeaderIdAsync(id, currentUserService.UserId ?? 0);
        await headerRepository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);

        await auditLogService.LogAsync("MasterData_PriceListHeaders", id, "SoftDelete", oldValues, null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    private async Task AddLinesAsync(long headerId, List<PriceListLineRequest> lines)
    {
        foreach (var line in lines)
        {
            await lineRepository.AddAsync(new PriceListLine
            {
                PriceListHeaderId = headerId,
                ItemId = line.ItemId,
                Price = line.Price,
                EffectiveDate = line.EffectiveDate,
                CreatedBy = currentUserService.UserId
            });
        }
    }

    private async Task<PriceListResponse> ToResponseAsync(PriceListHeader header)
    {
        var lines = await lineRepository.GetByHeaderIdAsync(header.Id);
        var lineResponses = lines.Select(l => new PriceListLineResponse(l.Id, l.ItemId, l.Price, l.EffectiveDate)).ToList();
        return new PriceListResponse(header.Id, header.Name, header.Type, header.CurrencyId, header.BusinessPartnerId, header.IsActive, header.CreatedAt, lineResponses);
    }
}
