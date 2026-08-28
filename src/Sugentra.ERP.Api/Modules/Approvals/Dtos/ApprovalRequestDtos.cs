namespace Sugentra.ERP.Api.Modules.Approvals.Dtos;

using System.ComponentModel.DataAnnotations;

public record ApprovalRequestLevelResponse(
    int LevelNumber,
    string Name,
    bool RequireAllApprovers,
    int RequiredApproverCount,
    int ApprovedCount,
    string Status,
    IReadOnlyList<long> EligibleApproverUserIds);

public record ApprovalRequestActionResponse(int LevelNumber, long ApproverUserId, string Action, string? Comment, DateTime ActionedAt);

public record ApprovalRequestResponse(
    long Id,
    string DocumentType,
    long DocumentId,
    string DocumentNumber,
    string Status,
    int CurrentLevelNumber,
    long RequestedBy,
    DateTime RequestedAt,
    DateTime? CompletedAt,
    IReadOnlyList<ApprovalRequestLevelResponse> Levels,
    IReadOnlyList<ApprovalRequestActionResponse> History);

public record ApprovalActionRequest(
    bool Approve,
    [Required, StringLength(8000, MinimumLength = 1)]
    string Comment);

// One row in the "My Approvals" inbox — a request currently waiting on the acting user at its current level.
public record ApprovalInboxItem(
    long RequestId,
    string DocumentType,
    long DocumentId,
    string DocumentNumber,
    int CurrentLevelNumber,
    string LevelName,
    long RequestedBy,
    DateTime RequestedAt,
    string ApproverTypeName,
    int AgingDays);

// Raw shape materialized directly from GetInboxForUserSql — AgingDays (business-day aware) is computed
// afterwards in ApprovalRequestUseCase, not in SQL, since it needs the holiday calendar.
public record ApprovalInboxRow(
    long RequestId,
    string DocumentType,
    long DocumentId,
    string DocumentNumber,
    int CurrentLevelNumber,
    string LevelName,
    long RequestedBy,
    DateTime RequestedAt,
    string ApproverTypeName);
