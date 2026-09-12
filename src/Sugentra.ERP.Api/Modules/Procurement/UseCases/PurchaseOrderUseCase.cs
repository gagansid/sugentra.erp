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

public class PurchaseOrderUseCase(
    GenericRepository<PurchaseOrder> orderRepository,
    IPurchaseOrderLineRepository lineRepository,
    IPurchaseOrderLineSourceRepository lineSourceRepository,
    IPurchaseOrderRequisitionRepository orderRequisitionRepository,
    GenericRepository<PurchaseRequisition> requisitionRepository,
    IPurchaseRequisitionLineRepository requisitionLineRepository,
    IItemDirectoryService itemDirectoryService,
    IBusinessPartnerDirectoryService businessPartnerDirectoryService,
    IDocumentNumberGeneratorService documentNumberGeneratorService,
    IApprovalService approvalService,
    IUserDirectoryService userDirectoryService,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService) : IPurchaseOrderReceiptService
{
    public async Task<IReadOnlyList<PurchaseOrderResponse>> GetAllAsync()
    {
        var orders = await orderRepository.GetAllAsync();
        var userCache = await ResolveUsersAsync(orders.Select(o => o.CreatedBy));
        return await Task.WhenAll(orders.Select(o => ToResponseAsync(o, userCache)));
    }

    public async Task<PurchaseOrderResponse?> GetByIdAsync(long id)
    {
        var order = await orderRepository.GetByIdAsync(id);
        return order is null ? null : await ToResponseAsync(order);
    }

    public async Task<Result<PurchaseOrderResponse>> CreateAsync(CreatePurchaseOrderRequest request)
    {
        var requisitionIds = (request.PurchaseRequisitionIds ?? []).Distinct().ToList();
        foreach (var requisitionId in requisitionIds)
        {
            // If sourced from a Purchase Requisition, it must already be Approved.
            var requisition = await requisitionRepository.GetByIdAsync(requisitionId);
            if (requisition is null)
            {
                return Result<PurchaseOrderResponse>.Failure($"Purchase requisition {requisitionId} not found.");
            }
            if (requisition.Status != "Approved")
            {
                return Result<PurchaseOrderResponse>.Failure("Purchase order can only be created from an Approved purchase requisition.");
            }
        }

        // Client is expected to send per-line Sources, but fall back to matching by ItemId
        // so the PR<->PO breakdown (used for OrderStatus/over-order checks) still gets recorded.
        await InferMissingSourcesAsync(requisitionIds, request.Lines);

        var quantityError = await ValidateSourceQuantitiesAsync(request.Lines, excludeOrderId: null);
        if (quantityError is not null)
        {
            return Result<PurchaseOrderResponse>.Failure(quantityError);
        }

        var number = await documentNumberGeneratorService.GetNextAsync("PurchaseOrder");

        var order = new PurchaseOrder
        {
            OrderNumber = number.FormattedNumber,
            VendorId = request.VendorId,
            CurrencyId = request.CurrencyId,
            PaymentTermDays = request.PaymentTermDays,
            OrderDate = request.OrderDate,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Status = "Draft",
            Notes = request.Notes,
            CreatedBy = currentUserService.UserId
        };

        var id = await orderRepository.AddAsync(order);
        await AddLinesAsync(id, request.Lines);
        foreach (var requisitionId in requisitionIds)
        {
            await orderRequisitionRepository.AddAsync(new PurchaseOrderRequisition
            {
                PurchaseOrderId = id,
                PurchaseRequisitionId = requisitionId,
                CreatedBy = currentUserService.UserId
            });
        }

        var response = await ToResponseAsync(order);
        await auditLogService.LogAsync("Procurement_PurchaseOrders", id, "Create", null, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<PurchaseOrderResponse>.Success(response);
    }

    public async Task<Result<PurchaseOrderResponse>> UpdateAsync(long id, UpdatePurchaseOrderRequest request)
    {
        var order = await orderRepository.GetByIdAsync(id);
        if (order is null)
        {
            return Result<PurchaseOrderResponse>.Failure($"Purchase order {id} not found.");
        }

        if (order.Status != "Draft")
        {
            return Result<PurchaseOrderResponse>.Failure("Only Draft purchase orders can be edited.");
        }

        var quantityError = await ValidateSourceQuantitiesAsync(request.Lines, excludeOrderId: id);
        if (quantityError is not null)
        {
            return Result<PurchaseOrderResponse>.Failure(quantityError);
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(order));

        order.VendorId = request.VendorId;
        order.CurrencyId = request.CurrencyId;
        order.PaymentTermDays = request.PaymentTermDays;
        order.OrderDate = request.OrderDate;
        order.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        order.Notes = request.Notes;
        order.UpdatedBy = currentUserService.UserId;
        order.UpdatedAt = DateTime.UtcNow;

        await orderRepository.UpdateAsync(order);

        // Full line-set replace — simplest correct behavior, avoids incremental add/remove diffing.
        var oldLines = await lineRepository.GetByOrderIdAsync(id);
        await lineSourceRepository.SoftDeleteByOrderLineIdsAsync(oldLines.Select(l => l.Id), currentUserService.UserId ?? 0);
        await lineRepository.SoftDeleteByOrderIdAsync(id, currentUserService.UserId ?? 0);
        await AddLinesAsync(id, request.Lines);

        var response = await ToResponseAsync(order);
        await auditLogService.LogAsync("Procurement_PurchaseOrders", id, "Update", oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<PurchaseOrderResponse>.Success(response);
    }

    public async Task<Result<PurchaseOrderResponse>> SubmitAsync(long id)
    {
        var order = await orderRepository.GetByIdAsync(id);
        if (order is null)
        {
            return Result<PurchaseOrderResponse>.Failure($"Purchase order {id} not found.");
        }

        if (order.Status != "Draft")
        {
            return Result<PurchaseOrderResponse>.Failure("Only Draft purchase orders can be submitted.");
        }

        var lines = await lineRepository.GetByOrderIdAsync(id);
        var amount = lines.Sum(l => l.Quantity * l.UnitPrice * (1 - (l.DiscountPercent / 100m)));

        var submission = await approvalService.SubmitForApprovalAsync(new ApprovalSubmissionRequest(
            "PurchaseOrder", id, order.OrderNumber, amount, order.CurrencyId, null, currentUserService.UserId ?? 0));

        if (submission.RequiresApproval)
        {
            // Status/CurrentApprovalLevel were already set by SetWaitingApprovalLevelAsync, called
            // synchronously from within SubmitForApprovalAsync — re-fetch since our copy is stale.
            var refreshed = await orderRepository.GetByIdAsync(id);
            return Result<PurchaseOrderResponse>.Success(await ToResponseAsync(refreshed!));
        }

        return await FinalizeApprovedAsync(order);
    }

    // Invoked by IApprovalDocumentHandler on submission and every time the request advances to a new level.
    public async Task SetWaitingApprovalLevelAsync(long id, string levelName)
    {
        var order = await orderRepository.GetByIdAsync(id);
        if (order is null) return;

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(order));

        order.Status = "WaitingApproval";
        order.CurrentApprovalLevel = levelName;
        order.UpdatedAt = DateTime.UtcNow;
        await orderRepository.UpdateAsync(order);

        var response = await ToResponseAsync(order);
        await auditLogService.LogAsync("Procurement_PurchaseOrders", id, "ApprovalLevelAdvanced", oldValues, JsonSerializer.Serialize(response), null);
    }

    // Invoked by IApprovalDocumentHandler once every approval level has signed off.
    public async Task CompleteApprovedAsync(long id)
    {
        var order = await orderRepository.GetByIdAsync(id);
        if (order is null || order.Status != "WaitingApproval") return;

        await FinalizeApprovedAsync(order);
    }

    // Rejection sends it back to Draft for revision/resubmission — the reason lives in the approval history.
    public async Task RejectAsync(long id)
    {
        var order = await orderRepository.GetByIdAsync(id);
        if (order is null || order.Status != "WaitingApproval") return;

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(order));

        order.Status = "Draft";
        order.CurrentApprovalLevel = null;
        order.UpdatedAt = DateTime.UtcNow;
        await orderRepository.UpdateAsync(order);

        var response = await ToResponseAsync(order);
        await auditLogService.LogAsync("Procurement_PurchaseOrders", id, "ApprovalRejected", oldValues, JsonSerializer.Serialize(response), null);
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var order = await orderRepository.GetByIdAsync(id);
        if (order is null)
        {
            return Result<bool>.Failure($"Purchase order {id} not found.");
        }

        if (order.Status != "Draft")
        {
            return Result<bool>.Failure("Only Draft purchase orders can be deleted.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(order));

        var linesToDelete = await lineRepository.GetByOrderIdAsync(id);
        await lineSourceRepository.SoftDeleteByOrderLineIdsAsync(linesToDelete.Select(l => l.Id), currentUserService.UserId ?? 0);
        await lineRepository.SoftDeleteByOrderIdAsync(id, currentUserService.UserId ?? 0);
        await orderRepository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);

        await auditLogService.LogAsync("Procurement_PurchaseOrders", id, "SoftDelete", oldValues, null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    // Called by Inventory (via this IPurchaseOrderReceiptService contract) after posting a Goods Receipt
    // referencing this PO, so Procurement can track received quantity without Inventory reading its tables.
    public async Task ApplyReceiptAsync(long purchaseOrderId, IReadOnlyList<PurchaseOrderReceiptLineUpdate> lines)
    {
        var order = await orderRepository.GetByIdAsync(purchaseOrderId);
        if (order is null) return;

        foreach (var lineUpdate in lines)
        {
            await lineRepository.UpdateReceivedQuantityAsync(lineUpdate.PurchaseOrderLineId, lineUpdate.ReceivedQuantity);
        }

        var allLines = await lineRepository.GetByOrderIdAsync(purchaseOrderId);
        var isFullyReceived = allLines.All(l => l.ReceivedQuantity >= l.Quantity);
        var isPartiallyReceived = allLines.Any(l => l.ReceivedQuantity > 0);

        order.Status = isFullyReceived ? "FullyReceived" : isPartiallyReceived ? "PartiallyReceived" : order.Status;
        order.UpdatedAt = DateTime.UtcNow;
        await orderRepository.UpdateAsync(order);

        await auditLogService.LogAsync("Procurement_PurchaseOrders", purchaseOrderId, "ReceiptApplied", null, JsonSerializer.Serialize(order.Status), null);
    }

    private async Task<Result<PurchaseOrderResponse>> FinalizeApprovedAsync(PurchaseOrder order)
    {
        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(order));

        order.Status = "Approved";
        order.CurrentApprovalLevel = null;
        order.UpdatedBy = currentUserService.UserId;
        order.UpdatedAt = DateTime.UtcNow;
        await orderRepository.UpdateAsync(order);

        var response = await ToResponseAsync(order);
        await auditLogService.LogAsync("Procurement_PurchaseOrders", order.Id, "Approved", oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<PurchaseOrderResponse>.Success(response);
    }

    // Best-effort fallback for lines the client submitted with no Sources breakdown: match by ItemId
    // against the selected requisitions' lines, allocating each requisition line's quantity in order.
    private async Task InferMissingSourcesAsync(List<long> requisitionIds, List<PurchaseOrderLineRequest> lines)
    {
        if (requisitionIds.Count == 0 || lines.All(l => l.Sources is { Count: > 0 })) return;

        var requisitionLines = new List<PurchaseRequisitionLine>();
        foreach (var requisitionId in requisitionIds)
        {
            requisitionLines.AddRange(await requisitionLineRepository.GetByRequisitionIdAsync(requisitionId));
        }
        var remaining = requisitionLines.ToDictionary(l => l.Id, l => l.Quantity);

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (line.Sources is { Count: > 0 }) continue;

            var toAllocate = line.Quantity;
            var sources = new List<PurchaseOrderLineSourceRequest>();
            foreach (var candidate in requisitionLines.Where(l => l.ItemId == line.ItemId))
            {
                if (toAllocate <= 0) break;
                var take = Math.Min(toAllocate, remaining[candidate.Id]);
                if (take <= 0) continue;
                sources.Add(new PurchaseOrderLineSourceRequest(candidate.RequisitionId, candidate.Id, take));
                remaining[candidate.Id] -= take;
                toAllocate -= take;
            }
            if (sources.Count > 0)
            {
                lines[i] = line with { Sources = sources };
            }
        }
    }

    // Sums requested quantity per PR line across the incoming lines and compares against what's already
    // committed to other orders (or all orders, for Create) plus each PR line's original requested quantity.
    private async Task<string?> ValidateSourceQuantitiesAsync(List<PurchaseOrderLineRequest> lines, long? excludeOrderId)
    {
        var requestedByLine = new Dictionary<long, decimal>();
        foreach (var line in lines)
        {
            foreach (var source in line.Sources ?? [])
            {
                if (source.PurchaseRequisitionLineId is not { } requisitionLineId) continue;
                requestedByLine[requisitionLineId] = requestedByLine.GetValueOrDefault(requisitionLineId) + source.Quantity;
            }
        }
        if (requestedByLine.Count == 0) return null;

        var alreadyOrdered = await lineSourceRepository.GetOrderedQuantityByRequisitionLineIdsAsync(requestedByLine.Keys, excludeOrderId);
        var requisitionLines = await requisitionLineRepository.GetByIdsAsync(requestedByLine.Keys);

        foreach (var (requisitionLineId, requestedQty) in requestedByLine)
        {
            var requisitionLine = requisitionLines.FirstOrDefault(l => l.Id == requisitionLineId);
            if (requisitionLine is null) continue;

            var already = alreadyOrdered.GetValueOrDefault(requisitionLineId);
            var remaining = requisitionLine.Quantity - already;
            if (requestedQty > remaining)
            {
                return $"Requested quantity ({requestedQty:N2}) for requisition line #{requisitionLineId} exceeds the remaining quantity ({remaining:N2}) available to order.";
            }
        }
        return null;
    }

    private async Task AddLinesAsync(long orderId, List<PurchaseOrderLineRequest> lines)
    {
        foreach (var line in lines)
        {
            var lineId = await lineRepository.AddAsync(new PurchaseOrderLine
            {
                PurchaseOrderId = orderId,
                ItemId = line.ItemId,
                WarehouseId = line.WarehouseId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                DiscountPercent = line.DiscountPercent,
                ReceivedQuantity = 0,
                CreatedBy = currentUserService.UserId
            });

            foreach (var source in line.Sources ?? [])
            {
                await lineSourceRepository.AddAsync(new PurchaseOrderLineSource
                {
                    PurchaseOrderLineId = lineId,
                    PurchaseRequisitionId = source.PurchaseRequisitionId,
                    PurchaseRequisitionLineId = source.PurchaseRequisitionLineId,
                    Quantity = source.Quantity,
                    CreatedBy = currentUserService.UserId
                });
            }
        }
    }

    private async Task<PurchaseOrderResponse> ToResponseAsync(PurchaseOrder order)
    {
        var userCache = await ResolveUsersAsync([order.CreatedBy]);
        return await ToResponseAsync(order, userCache);
    }

    private async Task<PurchaseOrderResponse> ToResponseAsync(PurchaseOrder order, IReadOnlyDictionary<long, UserDirectoryEntry> userCache)
    {
        var lines = await lineRepository.GetByOrderIdAsync(order.Id);
        var allSources = await lineSourceRepository.GetByOrderLineIdsAsync(lines.Select(l => l.Id));
        var requisitionNumberCache = new Dictionary<long, string?>();
        var lineResponses = await Task.WhenAll(lines.Select(async l =>
        {
            var item = await itemDirectoryService.GetSummaryAsync(l.ItemId);
            var sourceResponses = new List<PurchaseOrderLineSourceResponse>();
            foreach (var source in allSources.Where(s => s.PurchaseOrderLineId == l.Id))
            {
                string? requisitionNumber = null;
                if (source.PurchaseRequisitionId.HasValue)
                {
                    if (!requisitionNumberCache.TryGetValue(source.PurchaseRequisitionId.Value, out requisitionNumber))
                    {
                        var requisition = await requisitionRepository.GetByIdAsync(source.PurchaseRequisitionId.Value);
                        requisitionNumber = requisition?.RequisitionNumber;
                        requisitionNumberCache[source.PurchaseRequisitionId.Value] = requisitionNumber;
                    }
                }
                sourceResponses.Add(new PurchaseOrderLineSourceResponse(source.PurchaseRequisitionId, requisitionNumber, source.Quantity));
            }
            return new PurchaseOrderLineResponse(l.Id, l.ItemId, item?.Code, item?.Name, l.WarehouseId, l.Quantity, l.UnitPrice, l.DiscountPercent, l.ReceivedQuantity, sourceResponses);
        }));

        var vendor = await businessPartnerDirectoryService.GetSummaryAsync(order.VendorId);
        var createdByUser = order.CreatedBy.HasValue && userCache.TryGetValue(order.CreatedBy.Value, out var user) ? user : null;

        var orderRequisitions = await orderRequisitionRepository.GetByOrderIdAsync(order.Id);
        var sourceRequisitions = await Task.WhenAll(orderRequisitions.Select(async r =>
        {
            var requisition = await requisitionRepository.GetByIdAsync(r.PurchaseRequisitionId);
            return new PurchaseOrderSourceRequisitionResponse(r.PurchaseRequisitionId, requisition?.RequisitionNumber ?? $"PR-{r.PurchaseRequisitionId}");
        }));

        return new PurchaseOrderResponse(
            order.Id, order.OrderNumber, sourceRequisitions, order.VendorId, vendor?.Code, vendor?.Name,
            order.CurrencyId, order.PaymentTermDays, order.OrderDate, order.ExpectedDeliveryDate,
            order.Status, order.CurrentApprovalLevel, order.Notes,
            order.CreatedAt, order.CreatedBy, createdByUser?.FullName, lineResponses);
    }

    // Resolves each distinct CreatedBy id once (IUserDirectoryService has no batch lookup) to avoid N+1 calls on list endpoints.
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
