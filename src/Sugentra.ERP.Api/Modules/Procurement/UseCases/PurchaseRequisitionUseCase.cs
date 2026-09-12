using System.Text.Json;
using Sugentra.ERP.Api.Modules.Procurement.Dtos;
using Sugentra.ERP.Api.Modules.Procurement.Entities;
using Sugentra.ERP.Api.Modules.Procurement.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Logging;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Procurement.UseCases;

public class PurchaseRequisitionUseCase(
    GenericRepository<PurchaseRequisition> requisitionRepository,
    IPurchaseRequisitionLineRepository lineRepository,
    IPurchaseOrderRequisitionRepository orderRequisitionRepository,
    GenericRepository<PurchaseOrder> orderRepository,
    IPurchaseOrderLineSourceRepository lineSourceRepository,
    IItemDirectoryService itemDirectoryService,
    IDocumentNumberGeneratorService documentNumberGeneratorService,
    IApprovalService approvalService,
    IUserDirectoryService userDirectoryService,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    public async Task<IReadOnlyList<PurchaseRequisitionResponse>> GetAllAsync()
    {
        var requisitions = await requisitionRepository.GetAllAsync();
        var userCache = await ResolveUsersAsync(requisitions.SelectMany(r => new[] { r.CreatedBy, (long?)r.RequesterUserId }));
        var orderIdsByRequisition = await orderRequisitionRepository.GetOrderIdsByRequisitionIdsAsync(requisitions.Select(r => r.Id));
        var orderNumberCache = (await orderRepository.GetAllAsync()).ToDictionary(o => o.Id, o => o.OrderNumber);
        var orderedQtyByRequisition = await lineSourceRepository.GetOrderedQuantityByRequisitionIdsAsync(requisitions.Select(r => r.Id));
        return await Task.WhenAll(requisitions.Select(r => ToResponseAsync(r, userCache, orderIdsByRequisition, orderNumberCache, orderedQtyByRequisition)));
    }

    public async Task<PurchaseRequisitionResponse?> GetByIdAsync(long id)
    {
        var requisition = await requisitionRepository.GetByIdAsync(id);
        return requisition is null ? null : await ToResponseAsync(requisition);
    }

    public async Task<PurchaseRequisitionResponse> CreateAsync(CreatePurchaseRequisitionRequest request)
    {
        var number = await documentNumberGeneratorService.GetNextAsync("PurchaseRequisition");

        var requisition = new PurchaseRequisition
        {
            RequisitionNumber = number.FormattedNumber,
            RequesterUserId = currentUserService.UserId ?? 0,
            WarehouseId = request.WarehouseId,
            RequisitionDate = request.RequisitionDate,
            Status = "Draft",
            Notes = request.Notes,
            CreatedBy = currentUserService.UserId
        };

        var id = await requisitionRepository.AddAsync(requisition);
        await AddLinesAsync(id, request.Lines);

        var response = await ToResponseAsync(requisition);
        await auditLogService.LogAsync("Procurement_PurchaseRequisitions", id, "Create", null, JsonSerializer.Serialize(response), currentUserService.UserId);

        return response;
    }

    public async Task<Result<PurchaseRequisitionResponse>> UpdateAsync(long id, UpdatePurchaseRequisitionRequest request)
    {
        var requisition = await requisitionRepository.GetByIdAsync(id);
        if (requisition is null)
        {
            return Result<PurchaseRequisitionResponse>.Failure($"Purchase requisition {id} not found.");
        }

        if (requisition.Status != "Draft")
        {
            return Result<PurchaseRequisitionResponse>.Failure("Only Draft requisitions can be edited.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(requisition));

        requisition.WarehouseId = request.WarehouseId;
        requisition.RequisitionDate = request.RequisitionDate;
        requisition.Notes = request.Notes;
        requisition.UpdatedBy = currentUserService.UserId;
        requisition.UpdatedAt = DateTime.UtcNow;

        await requisitionRepository.UpdateAsync(requisition);

        // Full line-set replace — simplest correct behavior, avoids incremental add/remove diffing.
        await lineRepository.SoftDeleteByRequisitionIdAsync(id, currentUserService.UserId ?? 0);
        await AddLinesAsync(id, request.Lines);

        var response = await ToResponseAsync(requisition);
        await auditLogService.LogAsync("Procurement_PurchaseRequisitions", id, "Update", oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<PurchaseRequisitionResponse>.Success(response);
    }

    public async Task<Result<PurchaseRequisitionResponse>> SubmitAsync(long id)
    {
        var requisition = await requisitionRepository.GetByIdAsync(id);
        if (requisition is null)
        {
            return Result<PurchaseRequisitionResponse>.Failure($"Purchase requisition {id} not found.");
        }

        if (requisition.Status != "Draft")
        {
            return Result<PurchaseRequisitionResponse>.Failure("Only Draft requisitions can be submitted.");
        }

        var submission = await approvalService.SubmitForApprovalAsync(new ApprovalSubmissionRequest(
            "PurchaseRequisition", id, requisition.RequisitionNumber, null, null, requisition.WarehouseId, currentUserService.UserId ?? 0));

        if (submission.RequiresApproval)
        {
            // Status/CurrentApprovalLevel were already set by SetWaitingApprovalLevelAsync, called
            // synchronously from within SubmitForApprovalAsync — re-fetch since our copy is stale.
            var refreshed = await requisitionRepository.GetByIdAsync(id);
            return Result<PurchaseRequisitionResponse>.Success(await ToResponseAsync(refreshed!));
        }

        return await FinalizeApprovedAsync(requisition);
    }

    // Invoked by IApprovalDocumentHandler on submission and every time the request advances to a new level.
    public async Task SetWaitingApprovalLevelAsync(long id, string levelName)
    {
        var requisition = await requisitionRepository.GetByIdAsync(id);
        if (requisition is null) return;

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(requisition));

        requisition.Status = "WaitingApproval";
        requisition.CurrentApprovalLevel = levelName;
        requisition.UpdatedAt = DateTime.UtcNow;
        await requisitionRepository.UpdateAsync(requisition);

        var response = await ToResponseAsync(requisition);
        await auditLogService.LogAsync("Procurement_PurchaseRequisitions", id, "ApprovalLevelAdvanced", oldValues, JsonSerializer.Serialize(response), null);
    }

    // Invoked by IApprovalDocumentHandler once every approval level has signed off.
    public async Task CompleteApprovedAsync(long id)
    {
        var requisition = await requisitionRepository.GetByIdAsync(id);
        if (requisition is null || requisition.Status != "WaitingApproval") return;

        await FinalizeApprovedAsync(requisition);
    }

    // Rejection sends it back to Draft for revision/resubmission — the reason lives in the approval history.
    public async Task RejectAsync(long id)
    {
        var requisition = await requisitionRepository.GetByIdAsync(id);
        if (requisition is null || requisition.Status != "WaitingApproval") return;

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(requisition));

        requisition.Status = "Draft";
        requisition.CurrentApprovalLevel = null;
        requisition.UpdatedAt = DateTime.UtcNow;
        await requisitionRepository.UpdateAsync(requisition);

        var response = await ToResponseAsync(requisition);
        await auditLogService.LogAsync("Procurement_PurchaseRequisitions", id, "ApprovalRejected", oldValues, JsonSerializer.Serialize(response), null);
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var requisition = await requisitionRepository.GetByIdAsync(id);
        if (requisition is null)
        {
            return Result<bool>.Failure($"Purchase requisition {id} not found.");
        }

        if (requisition.Status != "Draft")
        {
            return Result<bool>.Failure("Only Draft requisitions can be deleted.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(requisition));

        await lineRepository.SoftDeleteByRequisitionIdAsync(id, currentUserService.UserId ?? 0);
        await requisitionRepository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);

        await auditLogService.LogAsync("Procurement_PurchaseRequisitions", id, "SoftDelete", oldValues, null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    private async Task<Result<PurchaseRequisitionResponse>> FinalizeApprovedAsync(PurchaseRequisition requisition)
    {
        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(requisition));

        requisition.Status = "Approved";
        requisition.CurrentApprovalLevel = null;
        requisition.UpdatedBy = currentUserService.UserId;
        requisition.UpdatedAt = DateTime.UtcNow;
        await requisitionRepository.UpdateAsync(requisition);

        var response = await ToResponseAsync(requisition);
        await auditLogService.LogAsync("Procurement_PurchaseRequisitions", requisition.Id, "Approved", oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<PurchaseRequisitionResponse>.Success(response);
    }

    private async Task AddLinesAsync(long requisitionId, List<PurchaseRequisitionLineRequest> lines)
    {
        foreach (var line in lines)
        {
            await lineRepository.AddAsync(new PurchaseRequisitionLine
            {
                RequisitionId = requisitionId,
                ItemId = line.ItemId,
                Quantity = line.Quantity,
                Notes = line.Notes,
                CreatedBy = currentUserService.UserId
            });
        }
    }

    private async Task<PurchaseRequisitionResponse> ToResponseAsync(PurchaseRequisition requisition)
    {
        var userCache = await ResolveUsersAsync([requisition.CreatedBy, requisition.RequesterUserId]);
        var orderIdsByRequisition = await orderRequisitionRepository.GetOrderIdsByRequisitionIdsAsync([requisition.Id]);
        var orderIds = orderIdsByRequisition.TryGetValue(requisition.Id, out var ids) ? ids : [];
        var orderNumberCache = new Dictionary<long, string>();
        foreach (var orderId in orderIds)
        {
            var order = await orderRepository.GetByIdAsync(orderId);
            if (order is not null) orderNumberCache[orderId] = order.OrderNumber;
        }
        var orderedQtyByRequisition = await lineSourceRepository.GetOrderedQuantityByRequisitionIdsAsync([requisition.Id]);
        return await ToResponseAsync(requisition, userCache, orderIdsByRequisition, orderNumberCache, orderedQtyByRequisition);
    }

    private async Task<PurchaseRequisitionResponse> ToResponseAsync(
        PurchaseRequisition requisition, IReadOnlyDictionary<long, UserDirectoryEntry> userCache,
        IReadOnlyDictionary<long, IReadOnlyList<long>> orderIdsByRequisition, IReadOnlyDictionary<long, string> orderNumberCache,
        IReadOnlyDictionary<long, decimal> orderedQtyByRequisition)
    {
        var lines = await lineRepository.GetByRequisitionIdAsync(requisition.Id);
        var orderedQtyByLine = await lineSourceRepository.GetOrderedQuantityByRequisitionLineIdsAsync(lines.Select(l => l.Id));
        var lineResponses = await Task.WhenAll(lines.Select(async l =>
        {
            var item = await itemDirectoryService.GetSummaryAsync(l.ItemId);
            var orderedLineQty = orderedQtyByLine.TryGetValue(l.Id, out var olq) ? olq : 0m;
            return new PurchaseRequisitionLineResponse(l.Id, l.ItemId, item?.Code, item?.Name, l.Quantity, l.Notes, orderedLineQty);
        }));

        var createdByUser = requisition.CreatedBy.HasValue && userCache.TryGetValue(requisition.CreatedBy.Value, out var createdBy) ? createdBy : null;
        var requesterUser = userCache.TryGetValue(requisition.RequesterUserId, out var requester) ? requester : null;

        var linkedOrders = orderIdsByRequisition.TryGetValue(requisition.Id, out var orderIds)
            ? orderIds.Where(orderNumberCache.ContainsKey).Select(orderId => new PurchaseRequisitionOrderResponse(orderId, orderNumberCache[orderId])).ToList()
            : [];

        var totalRequestedQty = lines.Sum(l => l.Quantity);
        var orderedQty = orderedQtyByRequisition.TryGetValue(requisition.Id, out var oq) ? oq : 0m;
        var orderStatus = orderedQty <= 0 ? "NotOrdered" : orderedQty >= totalRequestedQty ? "FullyOrdered" : "PartiallyOrdered";

        return new PurchaseRequisitionResponse(
            requisition.Id, requisition.RequisitionNumber, requisition.RequesterUserId, requesterUser?.FullName, requisition.WarehouseId,
            requisition.RequisitionDate, requisition.Status, requisition.CurrentApprovalLevel, requisition.Notes,
            requisition.CreatedAt, requisition.CreatedBy, createdByUser?.FullName, lineResponses, linkedOrders, orderStatus);
    }

    // Resolves each distinct user id once (IUserDirectoryService has no batch lookup) to avoid N+1 calls on list endpoints.
    private async Task<IReadOnlyDictionary<long, UserDirectoryEntry>> ResolveUsersAsync(IEnumerable<long?> userIds)
    {
        var distinctIds = userIds.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        if (distinctIds.Count == 0) return new Dictionary<long, UserDirectoryEntry>();

        var users = await Task.WhenAll(distinctIds.Select(userDirectoryService.GetByIdAsync));
        return distinctIds.Zip(users, (id, user) => (id, user))
            .Where(x => x.user is not null)
            .ToDictionary(x => x.id, x => x.user!);
    }
}
