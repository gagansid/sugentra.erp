using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Sugentra.ERP.Api.Modules.Approvals.Dtos;
using Sugentra.ERP.Api.Modules.Approvals.Entities;
using Sugentra.ERP.Api.Modules.Approvals.Repositories;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Logging;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Approvals.UseCases;

// The dynamic approval engine core: matches a document to a flow, builds the per-request level/approver
// snapshot, and drives level-by-level progression as approvers act. Reusable across any DocumentType.
public class ApprovalRequestUseCase(
    GenericRepository<ApprovalRequest> requestRepository,
    GenericRepository<ApprovalRequestLevel> requestLevelRepository,
    GenericRepository<ApprovalRequestLevelApprover> requestLevelApproverRepository,
    GenericRepository<ApprovalRequestAction> requestActionRepository,
    IApprovalFlowRepository flowRepository,
    IApprovalRequestRepository requestQueryRepository,
    IRoleMembershipService roleMembershipService,
    IUserDirectoryService userDirectoryService,
    IHolidayLookupService holidayLookupService,
    IServiceProvider serviceProvider,
    IAuditLogService auditLogService)
{
    public async Task<ApprovalSubmissionResult> SubmitAsync(ApprovalSubmissionRequest request)
    {
        var existing = await requestQueryRepository.GetActiveByDocumentAsync(request.DocumentType, request.DocumentId);
        if (existing is not null)
        {
            return new ApprovalSubmissionResult(true, existing.Id, "An approval request is already pending for this document.");
        }

        var flow = await MatchFlowAsync(request);
        if (flow is null)
        {
            return new ApprovalSubmissionResult(false, null, null);
        }

        var levels = await flowRepository.GetLevelsAsync(flow.Id);
        if (levels.Count == 0)
        {
            return new ApprovalSubmissionResult(false, null, null);
        }

        var approvalRequest = new ApprovalRequest
        {
            DocumentType = request.DocumentType,
            DocumentId = request.DocumentId,
            DocumentNumber = request.DocumentNumber,
            FlowDefinitionId = flow.Id,
            Amount = request.Amount,
            CurrencyId = request.CurrencyId,
            WarehouseId = request.WarehouseId,
            Status = "Pending",
            CurrentLevelNumber = levels.Min(l => l.LevelNumber),
            RequestedBy = request.RequestedByUserId,
            RequestedAt = DateTime.UtcNow,
            CreatedBy = request.RequestedByUserId
        };
        var requestId = await requestRepository.AddAsync(approvalRequest);

        foreach (var level in levels.OrderBy(l => l.LevelNumber))
        {
            var approvers = await flowRepository.GetApproversAsync(level.Id);
            var eligibleUserIds = await ResolveEligibleApproversAsync(approvers);

            var requestLevel = new ApprovalRequestLevel
            {
                RequestId = requestId,
                LevelNumber = level.LevelNumber,
                Name = level.Name,
                RequireAllApprovers = level.RequireAllApprovers,
                RequiredApproverCount = level.RequireAllApprovers ? Math.Max(eligibleUserIds.Count, 1) : 1,
                ApprovedCount = 0,
                Status = "Pending"
            };
            var requestLevelId = await requestLevelRepository.AddAsync(requestLevel);

            foreach (var userId in eligibleUserIds)
            {
                await requestLevelApproverRepository.AddAsync(new ApprovalRequestLevelApprover
                {
                    RequestLevelId = requestLevelId,
                    UserId = userId,
                    HasActed = false
                });
            }
        }

        await auditLogService.LogAsync("Approval_Requests", requestId, "Submitted", null,
            JsonSerializer.Serialize(request), request.RequestedByUserId);

        var firstLevel = levels.OrderBy(l => l.LevelNumber).First();
        await NotifyLevelChangedAsync(request.DocumentType, request.DocumentId, firstLevel.Name, firstLevel.LevelNumber, levels.Count);

        return new ApprovalSubmissionResult(true, requestId, "Submitted for approval.");
    }

    public async Task<Result<ApprovalRequestResponse>> ActAsync(long requestId, long approverUserId, bool approve, string? comment)
    {
        var request = await requestQueryRepository.GetByIdAsync(requestId);
        if (request is null) return Result<ApprovalRequestResponse>.Failure($"Approval request {requestId} not found.");
        if (request.Status != "Pending") return Result<ApprovalRequestResponse>.Failure("This request has already been completed.");

        var currentLevel = await requestQueryRepository.GetCurrentLevelAsync(requestId, request.CurrentLevelNumber);
        if (currentLevel is null) return Result<ApprovalRequestResponse>.Failure("Current approval level not found.");

        if (!await requestQueryRepository.IsEligibleApproverAsync(currentLevel.Id, approverUserId))
            return Result<ApprovalRequestResponse>.Failure("You are not an eligible approver for this level.");

        if (await requestQueryRepository.HasAlreadyActedAsync(requestId, currentLevel.LevelNumber, approverUserId))
            return Result<ApprovalRequestResponse>.Failure("You have already acted on this approval level.");

        if (!approve && string.IsNullOrWhiteSpace(comment))
            return Result<ApprovalRequestResponse>.Failure("Remarks are required when rejecting.");

        await requestActionRepository.AddAsync(new ApprovalRequestAction
        {
            RequestId = requestId,
            LevelNumber = currentLevel.LevelNumber,
            ApproverUserId = approverUserId,
            Action = approve ? "Approved" : "Rejected",
            Comment = comment,
            ActionedAt = DateTime.UtcNow,
            CreatedBy = approverUserId
        });
        await requestQueryRepository.MarkApproverActedAsync(currentLevel.Id, approverUserId);

        if (!approve)
        {
            currentLevel.Status = "Rejected";
            await requestLevelRepository.UpdateAsync(currentLevel);

            request.Status = "Rejected";
            request.CompletedAt = DateTime.UtcNow;
            request.UpdatedBy = approverUserId;
            request.UpdatedAt = DateTime.UtcNow;
            await requestRepository.UpdateAsync(request);

            await NotifyHandlerAsync(request, approved: false, comment);
            await auditLogService.LogAsync("Approval_Requests", requestId, "Rejected", null, comment, approverUserId);
            return Result<ApprovalRequestResponse>.Success((await GetDetailAsync(requestId))!);
        }

        currentLevel.ApprovedCount++;
        var levelComplete = currentLevel.ApprovedCount >= currentLevel.RequiredApproverCount;
        if (levelComplete)
        {
            currentLevel.Status = "Approved";
        }
        await requestLevelRepository.UpdateAsync(currentLevel);

        if (levelComplete)
        {
            var allLevels = await requestQueryRepository.GetLevelsAsync(requestId);
            var maxLevel = allLevels.Max(l => l.LevelNumber);
            if (currentLevel.LevelNumber >= maxLevel)
            {
                request.Status = "Approved";
                request.CompletedAt = DateTime.UtcNow;
                request.UpdatedBy = approverUserId;
                request.UpdatedAt = DateTime.UtcNow;
                await requestRepository.UpdateAsync(request);

                await NotifyHandlerAsync(request, approved: true, comment: null);
                await auditLogService.LogAsync("Approval_Requests", requestId, "Approved", null, null, approverUserId);
            }
            else
            {
                var nextLevelNumber = allLevels.Where(l => l.LevelNumber > currentLevel.LevelNumber).Min(l => l.LevelNumber);
                var nextLevel = allLevels.First(l => l.LevelNumber == nextLevelNumber);
                request.CurrentLevelNumber = nextLevelNumber;
                request.UpdatedBy = approverUserId;
                request.UpdatedAt = DateTime.UtcNow;
                await requestRepository.UpdateAsync(request);
                await auditLogService.LogAsync("Approval_Requests", requestId, "LevelAdvanced", null,
                    JsonSerializer.Serialize(new { NextLevel = nextLevelNumber }), approverUserId);

                await NotifyLevelChangedAsync(request.DocumentType, request.DocumentId, nextLevel.Name, nextLevel.LevelNumber, allLevels.Count);
            }
        }

        return Result<ApprovalRequestResponse>.Success((await GetDetailAsync(requestId))!);
    }

    public async Task<ApprovalStatusDto?> GetStatusAsync(string documentType, long documentId)
    {
        var request = await requestQueryRepository.GetActiveByDocumentAsync(documentType, documentId);
        if (request is null) return null;

        var levels = await requestQueryRepository.GetLevelsAsync(request.Id);
        return new ApprovalStatusDto(request.Id, request.Status, request.CurrentLevelNumber, levels.Count);
    }

    public async Task<ApprovalRequestResponse?> GetDetailAsync(long requestId)
    {
        var request = await requestQueryRepository.GetByIdAsync(requestId);
        if (request is null) return null;

        var levels = await requestQueryRepository.GetLevelsAsync(requestId);
        var approversByLevel = await requestQueryRepository.GetApproversByRequestLevelIdsAsync(levels.Select(l => l.Id));
        var history = await requestQueryRepository.GetHistoryAsync(requestId);

        var levelResponses = levels.OrderBy(l => l.LevelNumber).Select(l => new ApprovalRequestLevelResponse(
            l.LevelNumber, l.Name, l.RequireAllApprovers, l.RequiredApproverCount, l.ApprovedCount, l.Status,
            (approversByLevel.TryGetValue(l.Id, out var approvers) ? approvers : []).Select(a => a.UserId).ToList())).ToList();

        var historyResponses = history.Select(h => new ApprovalRequestActionResponse(
            h.LevelNumber, h.ApproverUserId, h.Action, h.Comment, h.ActionedAt)).ToList();

        return new ApprovalRequestResponse(
            request.Id, request.DocumentType, request.DocumentId, request.DocumentNumber, request.Status,
            request.CurrentLevelNumber, request.RequestedBy, request.RequestedAt, request.CompletedAt,
            levelResponses, historyResponses);
    }

    public async Task<IReadOnlyList<ApprovalInboxItem>> GetInboxAsync(long userId)
    {
        var rows = await requestQueryRepository.GetInboxForUserAsync(userId);
        if (rows.Count == 0)
        {
            return [];
        }

        var now = DateTime.UtcNow;
        var earliestRequestedAt = rows.Min(r => r.RequestedAt);
        var holidays = await holidayLookupService.GetHolidayDatesAsync(earliestRequestedAt, now);

        return rows.Select(r => new ApprovalInboxItem(
            r.RequestId, r.DocumentType, r.DocumentId, r.DocumentNumber, r.CurrentLevelNumber, r.LevelName,
            r.RequestedBy, r.RequestedAt, r.ApproverTypeName, CountBusinessDays(r.RequestedAt, now, holidays))).ToList();
    }

    // Counts full business days elapsed since RequestedAt (0 if still the same calendar day), excluding
    // weekends and the holiday calendar - used for the "Aging" column in the My Approvals inbox.
    private static int CountBusinessDays(DateTime from, DateTime to, IReadOnlySet<DateTime> holidays)
    {
        var days = 0;
        for (var date = from.Date.AddDays(1); date <= to.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday && !holidays.Contains(date))
            {
                days++;
            }
        }

        return days;
    }

    // Spans every submission cycle (a rejection followed by resubmission opens a new Approval_Requests row),
    // plus the still-pending approvers on whichever request is currently Pending, so the UI can render one timeline.
    public async Task<IReadOnlyList<ApprovalHistoryEntryDto>> GetDocumentHistoryAsync(string documentType, long documentId)
    {
        var requests = await requestQueryRepository.GetAllByDocumentAsync(documentType, documentId);
        var entries = new List<ApprovalHistoryEntryDto>();

        foreach (var request in requests)
        {
            var levels = await requestQueryRepository.GetLevelsAsync(request.Id);
            var levelsByNumber = levels.ToDictionary(l => l.LevelNumber);
            var actions = await requestQueryRepository.GetHistoryAsync(request.Id);

            foreach (var action in actions)
            {
                var levelName = levelsByNumber.TryGetValue(action.LevelNumber, out var actedLevel) ? actedLevel.Name : $"Level {action.LevelNumber}";
                var approver = await userDirectoryService.GetByIdAsync(action.ApproverUserId);
                entries.Add(new ApprovalHistoryEntryDto(
                    request.Id, action.LevelNumber, levelName, action.ApproverUserId, approver?.FullName,
                    action.Action, action.Comment, action.ActionedAt));
            }

            if (request.Status == "Pending" && levelsByNumber.TryGetValue(request.CurrentLevelNumber, out var currentLevel))
            {
                var pendingApprovers = await requestQueryRepository.GetApproversAsync(currentLevel.Id);
                foreach (var pending in pendingApprovers.Where(a => !a.HasActed))
                {
                    var user = await userDirectoryService.GetByIdAsync(pending.UserId);
                    entries.Add(new ApprovalHistoryEntryDto(
                        request.Id, currentLevel.LevelNumber, currentLevel.Name, pending.UserId, user?.FullName,
                        "Waiting", null, null));
                }
            }
        }

        return entries;
    }

    private async Task<ApprovalFlowDefinition?> MatchFlowAsync(ApprovalSubmissionRequest request)
    {
        var flows = await flowRepository.GetActiveByApproverTypeAsync(request.DocumentType);
        return flows.FirstOrDefault(f =>
            (f.MinAmount is null || request.Amount is null || request.Amount >= f.MinAmount) &&
            (f.MaxAmount is null || request.Amount is null || request.Amount <= f.MaxAmount) &&
            (f.CurrencyId is null || f.CurrencyId == request.CurrencyId) &&
            (f.WarehouseId is null || f.WarehouseId == request.WarehouseId));
    }

    private async Task<List<long>> ResolveEligibleApproversAsync(IReadOnlyList<ApprovalFlowLevelApprover> approvers)
    {
        var userIds = new HashSet<long>(approvers.Where(a => a.UserId.HasValue).Select(a => a.UserId!.Value));
        foreach (var roleId in approvers.Where(a => a.RoleId.HasValue).Select(a => a.RoleId!.Value).Distinct())
        {
            foreach (var userId in await roleMembershipService.GetUserIdsInRoleAsync(roleId))
            {
                userIds.Add(userId);
            }
        }
        return userIds.ToList();
    }

    private async Task NotifyHandlerAsync(ApprovalRequest request, bool approved, string? comment)
    {
        var handler = ResolveHandler(request.DocumentType);
        if (handler is null) return;

        if (approved)
        {
            await handler.OnApprovedAsync(request.DocumentId);
        }
        else
        {
            await handler.OnRejectedAsync(request.DocumentId, comment);
        }
    }

    private async Task NotifyLevelChangedAsync(string documentType, long documentId, string levelName, int levelNumber, int totalLevelCount)
    {
        var handler = ResolveHandler(documentType);
        if (handler is null) return;

        await handler.OnLevelChangedAsync(documentId, levelName, levelNumber, totalLevelCount);
    }

    // Resolved lazily (not constructor-injected) to avoid a circular dependency: document handlers
    // live in the consuming module and depend back on services that depend on this use case.
    private IApprovalDocumentHandler? ResolveHandler(string documentType) =>
        serviceProvider.GetServices<IApprovalDocumentHandler>().FirstOrDefault(h => h.DocumentType == documentType);
}
