using System.Text.Json;
using Sugentra.ERP.Api.Modules.Approvals.Dtos;
using Sugentra.ERP.Api.Modules.Approvals.Entities;
using Sugentra.ERP.Api.Modules.Approvals.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Logging;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Approvals.UseCases;

// Manages the reusable "jenjang" (tiered) approval flow configurations — the admin-facing side of the engine.
public class ApprovalFlowUseCase(
    GenericRepository<ApprovalFlowDefinition> flowRepository,
    IApprovalFlowRepository approvalFlowRepository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    public async Task<IReadOnlyList<ApprovalFlowDefinitionResponse>> GetAllAsync()
    {
        var flows = await flowRepository.GetAllAsync();
        if (flows.Count == 0) return [];

        var levelsByFlow = await approvalFlowRepository.GetLevelsByFlowDefinitionIdsAsync(flows.Select(f => f.Id));
        var allLevelIds = levelsByFlow.Values.SelectMany(l => l).Select(l => l.Id).ToList();
        var approversByLevel = await approvalFlowRepository.GetApproversByFlowLevelIdsAsync(allLevelIds);

        return flows.Select(f => ToResponse(f, levelsByFlow, approversByLevel)).ToList();
    }

    public async Task<ApprovalFlowDefinitionResponse?> GetByIdAsync(long id)
    {
        var flow = await flowRepository.GetByIdAsync(id);
        if (flow is null) return null;

        var levels = await approvalFlowRepository.GetLevelsAsync(id);
        var approversByLevel = new Dictionary<long, IReadOnlyList<ApprovalFlowLevelApprover>>();
        foreach (var level in levels)
        {
            approversByLevel[level.Id] = await approvalFlowRepository.GetApproversAsync(level.Id);
        }

        return ToResponse(flow, new Dictionary<long, IReadOnlyList<ApprovalFlowLevel>> { [id] = levels }, approversByLevel);
    }

    public async Task<Result<ApprovalFlowDefinitionResponse>> CreateAsync(SaveApprovalFlowDefinitionRequest request)
    {
        var validationError = await ValidateAsync(request);
        if (validationError is not null) return Result<ApprovalFlowDefinitionResponse>.Failure(validationError);

        var flow = new ApprovalFlowDefinition
        {
            DocumentType = request.DocumentType,
            Name = request.Name,
            MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount,
            CurrencyId = request.CurrencyId,
            WarehouseId = request.WarehouseId,
            Priority = request.Priority,
            IsActive = request.IsActive,
            CreatedBy = currentUserService.UserId
        };
        var id = await flowRepository.AddAsync(flow);
        await SaveLevelsAsync(id, request.Levels);

        await auditLogService.LogAsync("Approval_FlowDefinitions", id, "Create", null, JsonSerializer.Serialize(request), currentUserService.UserId);
        return Result<ApprovalFlowDefinitionResponse>.Success((await GetByIdAsync(id))!);
    }

    public async Task<Result<ApprovalFlowDefinitionResponse>> UpdateAsync(long id, SaveApprovalFlowDefinitionRequest request)
    {
        var existing = await flowRepository.GetByIdAsync(id);
        if (existing is null) return Result<ApprovalFlowDefinitionResponse>.Failure($"Approval flow {id} not found.");

        var validationError = await ValidateAsync(request);
        if (validationError is not null) return Result<ApprovalFlowDefinitionResponse>.Failure(validationError);

        var oldValues = JsonSerializer.Serialize(existing);

        existing.DocumentType = request.DocumentType;
        existing.Name = request.Name;
        existing.MinAmount = request.MinAmount;
        existing.MaxAmount = request.MaxAmount;
        existing.CurrencyId = request.CurrencyId;
        existing.WarehouseId = request.WarehouseId;
        existing.Priority = request.Priority;
        existing.IsActive = request.IsActive;
        existing.UpdatedBy = currentUserService.UserId;
        existing.UpdatedAt = DateTime.UtcNow;
        await flowRepository.UpdateAsync(existing);
        await SaveLevelsAsync(id, request.Levels);

        await auditLogService.LogAsync("Approval_FlowDefinitions", id, "Update", oldValues, JsonSerializer.Serialize(request), currentUserService.UserId);
        return Result<ApprovalFlowDefinitionResponse>.Success((await GetByIdAsync(id))!);
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var existing = await flowRepository.GetByIdAsync(id);
        if (existing is null) return Result<bool>.Failure($"Approval flow {id} not found.");

        await flowRepository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);
        await auditLogService.LogAsync("Approval_FlowDefinitions", id, "SoftDelete", JsonSerializer.Serialize(existing), null, currentUserService.UserId);
        return Result<bool>.Success(true);
    }

    private async Task SaveLevelsAsync(long flowDefinitionId, IReadOnlyList<ApprovalFlowLevelDto> levelDtos)
    {
        var levels = levelDtos.Select(l => new ApprovalFlowLevel
        {
            LevelNumber = l.LevelNumber,
            Name = l.Name,
            RequireAllApprovers = l.RequireAllApprovers
        }).ToList();

        var approversByLevelNumber = levelDtos.ToDictionary(
            l => l.LevelNumber,
            l => l.Approvers.Select(a => new ApprovalFlowLevelApprover { RoleId = a.RoleId, UserId = a.UserId }).ToList());

        await approvalFlowRepository.ReplaceLevelsAsync(flowDefinitionId, levels, approversByLevelNumber);
    }

    private async Task<string?> ValidateAsync(SaveApprovalFlowDefinitionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentType)) return "DocumentType is required.";
        if (string.IsNullOrWhiteSpace(request.Name)) return "Name is required.";
        if (request.Levels.Count == 0) return "At least one approval level is required.";
        if (request.Levels.Any(l => l.Approvers.Count == 0)) return "Every level must have at least one approver (role or user).";
        if (request.Levels.Any(l => l.Approvers.Any(a => a.RoleId is null && a.UserId is null)))
            return "Each level approver must reference either a Role or a User.";

        var roleIds = request.Levels.SelectMany(l => l.Approvers).Where(a => a.RoleId.HasValue).Select(a => a.RoleId!.Value).Distinct();
        foreach (var roleId in roleIds)
        {
            if (!await approvalFlowRepository.IsRoleAuthorizedForDocumentTypeAsync(roleId, request.DocumentType))
            {
                return $"Role {roleId} is not authorized to approve '{request.DocumentType}'. Add it under Approval Role Categories first.";
            }
        }

        return null;
    }

    private static ApprovalFlowDefinitionResponse ToResponse(
        ApprovalFlowDefinition flow,
        IReadOnlyDictionary<long, IReadOnlyList<ApprovalFlowLevel>> levelsByFlow,
        IReadOnlyDictionary<long, IReadOnlyList<ApprovalFlowLevelApprover>> approversByLevel)
    {
        var levels = levelsByFlow.TryGetValue(flow.Id, out var flowLevels) ? flowLevels : [];
        var levelDtos = levels
            .OrderBy(l => l.LevelNumber)
            .Select(l => new ApprovalFlowLevelDto(
                l.LevelNumber,
                l.Name,
                l.RequireAllApprovers,
                (approversByLevel.TryGetValue(l.Id, out var approvers) ? approvers : [])
                    .Select(a => new ApprovalFlowLevelApproverDto(a.RoleId, a.UserId)).ToList()))
            .ToList();

        return new ApprovalFlowDefinitionResponse(
            flow.Id, flow.DocumentType, flow.Name, flow.MinAmount, flow.MaxAmount,
            flow.CurrencyId, flow.WarehouseId, flow.Priority, flow.IsActive, levelDtos);
    }
}
