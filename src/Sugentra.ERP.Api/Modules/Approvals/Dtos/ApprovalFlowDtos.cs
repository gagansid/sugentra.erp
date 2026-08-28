namespace Sugentra.ERP.Api.Modules.Approvals.Dtos;

public record ApprovalFlowLevelApproverDto(long? RoleId, long? UserId);

public record ApprovalFlowLevelDto(int LevelNumber, string Name, bool RequireAllApprovers, IReadOnlyList<ApprovalFlowLevelApproverDto> Approvers);

public record ApprovalFlowDefinitionResponse(
    long Id,
    string ApproverType,
    string Name,
    decimal? MinAmount,
    decimal? MaxAmount,
    long? CurrencyId,
    long? WarehouseId,
    int Priority,
    bool IsActive,
    IReadOnlyList<ApprovalFlowLevelDto> Levels);

public record SaveApprovalFlowDefinitionRequest(
    string ApproverType,
    string Name,
    decimal? MinAmount,
    decimal? MaxAmount,
    long? CurrencyId,
    long? WarehouseId,
    int Priority,
    bool IsActive,
    IReadOnlyList<ApprovalFlowLevelDto> Levels);
