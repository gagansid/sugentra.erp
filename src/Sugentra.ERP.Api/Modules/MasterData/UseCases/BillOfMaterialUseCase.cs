using System.Text.Json;
using Sugentra.ERP.Api.Modules.MasterData.Dtos;
using Sugentra.ERP.Api.Modules.MasterData.Entities;
using Sugentra.ERP.Api.Modules.MasterData.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Logging;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData.UseCases;

public class BillOfMaterialUseCase(
    GenericRepository<BillOfMaterial> bomRepository,
    IBillOfMaterialItemRepository bomItemRepository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    public async Task<IReadOnlyList<BillOfMaterialResponse>> GetAllAsync()
    {
        var boms = await bomRepository.GetAllAsync();
        return await Task.WhenAll(boms.Select(ToResponseAsync));
    }

    public async Task<BillOfMaterialResponse?> GetByIdAsync(long id)
    {
        var bom = await bomRepository.GetByIdAsync(id);
        return bom is null ? null : await ToResponseAsync(bom);
    }

    public async Task<BillOfMaterialResponse> CreateAsync(CreateBillOfMaterialRequest request)
    {
        var bom = new BillOfMaterial
        {
            ItemId = request.ItemId,
            Name = request.Name,
            Description = request.Description,
            CreatedBy = currentUserService.UserId
        };

        var id = await bomRepository.AddAsync(bom);
        await AddLinesAsync(id, request.Lines);

        var response = await ToResponseAsync(bom);
        await auditLogService.LogAsync("MasterData_BillOfMaterials", id, "Create", null, JsonSerializer.Serialize(response), currentUserService.UserId);

        return response;
    }

    public async Task<Result<BillOfMaterialResponse>> UpdateAsync(long id, UpdateBillOfMaterialRequest request)
    {
        var bom = await bomRepository.GetByIdAsync(id);
        if (bom is null)
        {
            return Result<BillOfMaterialResponse>.Failure($"BillOfMaterial {id} not found.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(bom));

        bom.Name = request.Name;
        bom.Description = request.Description;
        bom.IsActive = request.IsActive;
        bom.UpdatedBy = currentUserService.UserId;
        bom.UpdatedAt = DateTime.UtcNow;

        await bomRepository.UpdateAsync(bom);

        // Full line-set replace — simplest correct behavior, avoids incremental add/remove diffing.
        await bomItemRepository.SoftDeleteByBomIdAsync(id, currentUserService.UserId ?? 0);
        await AddLinesAsync(id, request.Lines);

        var response = await ToResponseAsync(bom);
        await auditLogService.LogAsync("MasterData_BillOfMaterials", id, "Update", oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<BillOfMaterialResponse>.Success(response);
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var bom = await bomRepository.GetByIdAsync(id);
        if (bom is null)
        {
            return Result<bool>.Failure($"BillOfMaterial {id} not found.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(bom));

        await bomItemRepository.SoftDeleteByBomIdAsync(id, currentUserService.UserId ?? 0);
        await bomRepository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);

        await auditLogService.LogAsync("MasterData_BillOfMaterials", id, "SoftDelete", oldValues, null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    private async Task AddLinesAsync(long bomId, List<BillOfMaterialLineRequest> lines)
    {
        foreach (var line in lines)
        {
            await bomItemRepository.AddAsync(new BillOfMaterialItem
            {
                BillOfMaterialId = bomId,
                ComponentItemId = line.ComponentItemId,
                Quantity = line.Quantity,
                UnitOfMeasurementId = line.UnitOfMeasurementId,
                Notes = line.Notes,
                CreatedBy = currentUserService.UserId
            });
        }
    }

    private async Task<BillOfMaterialResponse> ToResponseAsync(BillOfMaterial bom)
    {
        var lines = await bomItemRepository.GetByBomIdAsync(bom.Id);
        var lineResponses = lines.Select(l => new BillOfMaterialLineResponse(l.Id, l.ComponentItemId, l.Quantity, l.UnitOfMeasurementId, l.Notes)).ToList();
        return new BillOfMaterialResponse(bom.Id, bom.ItemId, bom.Name, bom.Description, bom.IsActive, bom.CreatedAt, lineResponses);
    }
}
