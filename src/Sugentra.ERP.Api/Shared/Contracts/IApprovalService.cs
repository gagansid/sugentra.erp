namespace Sugentra.ERP.Api.Shared.Contracts;

// Cross-module contract: any module submits a document for approval without referencing Modules/Approvals directly.
public record ApprovalSubmissionRequest(
    string DocumentType,
    long DocumentId,
    string DocumentNumber,
    decimal? Amount,
    long? CurrencyId,
    long? WarehouseId,
    long RequestedByUserId);

public record ApprovalSubmissionResult(bool RequiresApproval, long? ApprovalRequestId, string? Message);

public record ApprovalStatusDto(
    long ApprovalRequestId,
    string Status,
    int CurrentLevelNumber,
    int TotalLevels);

// One row in a document's approval trail: either a completed action (Approved/Rejected) or a still-pending
// approver on the current level (Status = "Waiting"), so callers can render a single unified timeline.
public record ApprovalHistoryEntryDto(
    long RequestId,
    int LevelNumber,
    string LevelName,
    long? ApproverUserId,
    string? ApproverName,
    string Status,
    string? Comment,
    DateTime? ActionedAt);

public interface IApprovalService
{
    /// Returns RequiresApproval = false when no active flow matches the document — caller should proceed with its own direct transition.
    Task<ApprovalSubmissionResult> SubmitForApprovalAsync(ApprovalSubmissionRequest request);

    Task<ApprovalStatusDto?> GetStatusAsync(string documentType, long documentId);

    // Full trail across every submission cycle for the document (a rejection-then-resubmit creates a new
    // Approval_Requests row), ordered oldest to newest.
    Task<IReadOnlyList<ApprovalHistoryEntryDto>> GetHistoryAsync(string documentType, long documentId);
}

// Implemented by the module that owns the document type (e.g. Inventory) so Approvals can call back
// when a request finishes, without Approvals referencing that module's namespace.
public interface IApprovalDocumentHandler
{
    string DocumentType { get; }

    Task OnApprovedAsync(long documentId);

    Task OnRejectedAsync(long documentId, string? reason);

    // Fired on submission (first level) and each time the request advances to a new level, so the owning
    // module can reflect the current approver stage (e.g. "Waiting Approval - {levelName}") on its document.
    Task OnLevelChangedAsync(long documentId, string levelName, int levelNumber, int totalLevelCount);
}
