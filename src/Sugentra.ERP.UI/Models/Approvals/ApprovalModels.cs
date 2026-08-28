namespace Sugentra.ERP.UI.Models.Approvals;

public record ApprovalFlowLevelApproverDto(long? RoleId, long? UserId);

public record ApprovalFlowLevelDto(int LevelNumber, string Name, bool RequireAllApprovers, IReadOnlyList<ApprovalFlowLevelApproverDto> Approvers);

public record ApprovalFlowDefinitionResponse(
    long Id, string ApproverType, string Name, decimal? MinAmount, decimal? MaxAmount,
    long? CurrencyId, long? WarehouseId, int Priority, bool IsActive, IReadOnlyList<ApprovalFlowLevelDto> Levels);

public record SaveApprovalFlowDefinitionRequest(
    string ApproverType, string Name, decimal? MinAmount, decimal? MaxAmount,
    long? CurrencyId, long? WarehouseId, int Priority, bool IsActive, IReadOnlyList<ApprovalFlowLevelDto> Levels);

public record ApprovalRequestLevelResponse(
    int LevelNumber, string Name, bool RequireAllApprovers, int RequiredApproverCount, int ApprovedCount,
    string Status, IReadOnlyList<long> EligibleApproverUserIds);

public record ApprovalRequestActionResponse(int LevelNumber, long ApproverUserId, string Action, string? Comment, DateTime ActionedAt);

public record ApproverTypeOption(string ApproverType, string? Description);


public record ApprovalRequestResponse(
    long Id, string DocumentType, long DocumentId, string DocumentNumber, string Status, int CurrentLevelNumber,
    long RequestedBy, DateTime RequestedAt, DateTime? CompletedAt,
    IReadOnlyList<ApprovalRequestLevelResponse> Levels, IReadOnlyList<ApprovalRequestActionResponse> History);

public record ApprovalActionRequest(bool Approve, string? Comment);

public record ApprovalInboxItem(long RequestId, string DocumentType, long DocumentId, string DocumentNumber, int CurrentLevelNumber, string LevelName, long RequestedBy, DateTime RequestedAt, string ApproverTypeName, int AgingDays);

public record ApprovalRoleCategoryResponse(long Id, long RoleId, string ApproverType, string? Description = null);

// One row per Approver Type, grouping every ApprovalRoleCategoryResponse that shares the same ApproverType.
public record ApprovalRoleCategoryGroupViewModel(string ApproverType, IReadOnlyList<ApprovalRoleCategoryResponse> Entries);

// One row in a document's approval timeline: a completed action (Approved/Rejected) or a still-pending
// approver on the current level (Status = "Waiting").
public record ApprovalHistoryEntry(
    long RequestId, int LevelNumber, string LevelName, long? ApproverUserId, string? ApproverName,
    string Status, string? Comment, DateTime? ActionedAt);
