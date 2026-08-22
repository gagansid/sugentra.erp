using System.Text.Json;
using Sugentra.ERP.Api.Modules.Inventory.Dtos;
using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Modules.Inventory.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Logging;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Inventory.UseCases;

public class GoodsReceiptUseCase(
    GenericRepository<GoodsReceipt> receiptRepository,
    IGoodsReceiptLineRepository lineRepository,
    GenericRepository<StockLedger> ledgerRepository,
    IStockBalanceRepository balanceRepository,
    IItemDirectoryService itemDirectoryService,
    IDocumentNumberGeneratorService documentNumberGeneratorService,
    IApprovalService approvalService,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    public async Task<IReadOnlyList<GoodsReceiptResponse>> GetAllAsync()
    {
        var receipts = await receiptRepository.GetAllAsync();
        return await Task.WhenAll(receipts.Select(ToResponseAsync));
    }

    public async Task<GoodsReceiptResponse?> GetByIdAsync(long id)
    {
        var receipt = await receiptRepository.GetByIdAsync(id);
        return receipt is null ? null : await ToResponseAsync(receipt);
    }

    public async Task<Result<GoodsReceiptResponse>> CreateAsync(CreateGoodsReceiptRequest request)
    {
        var duplicateBatchError = ValidateNoDuplicateBatches(request.Lines);
        if (duplicateBatchError is not null)
        {
            return Result<GoodsReceiptResponse>.Failure(duplicateBatchError);
        }

        var number = await documentNumberGeneratorService.GetNextAsync("GoodsReceipt");

        var receipt = new GoodsReceipt
        {
            ReceiptNumber = number.FormattedNumber,
            WarehouseId = request.WarehouseId,
            VendorReference = request.VendorReference,
            ReceiptDate = request.ReceiptDate,
            Status = "Draft",
            Notes = request.Notes,
            CreatedBy = currentUserService.UserId
        };

        var id = await receiptRepository.AddAsync(receipt);
        await AddLinesAsync(id, request.Lines);

        var response = await ToResponseAsync(receipt);
        await auditLogService.LogAsync("Inventory_GoodsReceipts", id, "Create", null, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<GoodsReceiptResponse>.Success(response);
    }

    // A batch is a single lot — using it twice in the same document is almost certainly a data-entry mistake.
    private static string? ValidateNoDuplicateBatches(List<GoodsReceiptLineRequest> lines)
    {
        var duplicateBatchId = lines
            .GroupBy(l => l.BatchId)
            .FirstOrDefault(g => g.Count() > 1)?.Key;

        return duplicateBatchId is null
            ? null
            : $"Batch {duplicateBatchId} is used in more than one line. Combine the quantities into a single line instead.";
    }

    public async Task<Result<GoodsReceiptResponse>> UpdateAsync(long id, UpdateGoodsReceiptRequest request)
    {
        var receipt = await receiptRepository.GetByIdAsync(id);
        if (receipt is null)
        {
            return Result<GoodsReceiptResponse>.Failure($"GoodsReceipt {id} not found.");
        }

        if (receipt.Status != "Draft")
        {
            return Result<GoodsReceiptResponse>.Failure("Only Draft goods receipts can be edited.");
        }

        var duplicateBatchError = ValidateNoDuplicateBatches(request.Lines);
        if (duplicateBatchError is not null)
        {
            return Result<GoodsReceiptResponse>.Failure(duplicateBatchError);
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(receipt));

        receipt.WarehouseId = request.WarehouseId;
        receipt.VendorReference = request.VendorReference;
        receipt.ReceiptDate = request.ReceiptDate;
        receipt.Notes = request.Notes;
        receipt.UpdatedBy = currentUserService.UserId;
        receipt.UpdatedAt = DateTime.UtcNow;

        await receiptRepository.UpdateAsync(receipt);

        // Full line-set replace — simplest correct behavior, avoids incremental add/remove diffing.
        await lineRepository.SoftDeleteByReceiptIdAsync(id, currentUserService.UserId ?? 0);
        await AddLinesAsync(id, request.Lines);

        var response = await ToResponseAsync(receipt);
        await auditLogService.LogAsync("Inventory_GoodsReceipts", id, "Update", oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<GoodsReceiptResponse>.Success(response);
    }

    public async Task<Result<GoodsReceiptResponse>> PostAsync(long id)
    {
        var receipt = await receiptRepository.GetByIdAsync(id);
        if (receipt is null)
        {
            return Result<GoodsReceiptResponse>.Failure($"GoodsReceipt {id} not found.");
        }

        if (receipt.Status != "Draft")
        {
            return Result<GoodsReceiptResponse>.Failure("Only Draft goods receipts can be posted.");
        }

        var lines = await lineRepository.GetByReceiptIdAsync(id);
        var amount = lines.Sum(l => l.Quantity * l.UnitCost);

        var submission = await approvalService.SubmitForApprovalAsync(new ApprovalSubmissionRequest(
            "GoodsReceipt", id, receipt.ReceiptNumber, amount, null, receipt.WarehouseId, currentUserService.UserId ?? 0));

        if (submission.RequiresApproval)
        {
            // Status/CurrentApprovalLevel were already set by SetWaitingApprovalLevelAsync, called
            // synchronously from within SubmitForApprovalAsync — re-fetch since our copy is stale.
            var refreshed = await receiptRepository.GetByIdAsync(id);
            return Result<GoodsReceiptResponse>.Success(await ToResponseAsync(refreshed!));
        }

        return await FinalizePostAsync(receipt);
    }

    // Invoked by IApprovalDocumentHandler on submission and every time the request advances to a new level.
    public async Task SetWaitingApprovalLevelAsync(long id, string levelName)
    {
        var receipt = await receiptRepository.GetByIdAsync(id);
        if (receipt is null) return;

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(receipt));

        receipt.Status = "WaitingApproval";
        receipt.CurrentApprovalLevel = levelName;
        receipt.UpdatedAt = DateTime.UtcNow;
        await receiptRepository.UpdateAsync(receipt);

        var response = await ToResponseAsync(receipt);
        await auditLogService.LogAsync("Inventory_GoodsReceipts", id, "ApprovalLevelAdvanced", oldValues, JsonSerializer.Serialize(response), null);
    }

    // Invoked by IApprovalDocumentHandler once every approval level has signed off.
    public async Task CompleteApprovedPostAsync(long id)
    {
        var receipt = await receiptRepository.GetByIdAsync(id);
        if (receipt is null || receipt.Status != "WaitingApproval") return;

        await FinalizePostAsync(receipt);
    }

    // Rejection sends it back to Draft for revision/resubmission — the reason lives in the approval history.
    public async Task RejectPostAsync(long id)
    {
        var receipt = await receiptRepository.GetByIdAsync(id);
        if (receipt is null || receipt.Status != "WaitingApproval") return;

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(receipt));

        receipt.Status = "Draft";
        receipt.CurrentApprovalLevel = null;
        receipt.UpdatedAt = DateTime.UtcNow;
        await receiptRepository.UpdateAsync(receipt);

        var response = await ToResponseAsync(receipt);
        await auditLogService.LogAsync("Inventory_GoodsReceipts", id, "ApprovalRejected", oldValues, JsonSerializer.Serialize(response), null);
    }

    private async Task<Result<GoodsReceiptResponse>> FinalizePostAsync(GoodsReceipt receipt)
    {
        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(receipt));

        receipt.Status = "Posted";
        receipt.CurrentApprovalLevel = null;
        receipt.UpdatedBy = currentUserService.UserId;
        receipt.UpdatedAt = DateTime.UtcNow;
        await receiptRepository.UpdateAsync(receipt);

        await ApplyStockReceiptAsync(receipt);

        var response = await ToResponseAsync(receipt);
        await auditLogService.LogAsync("Inventory_GoodsReceipts", receipt.Id, "Posted", oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<GoodsReceiptResponse>.Success(response);
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var receipt = await receiptRepository.GetByIdAsync(id);
        if (receipt is null)
        {
            return Result<bool>.Failure($"GoodsReceipt {id} not found.");
        }

        if (receipt.Status != "Draft")
        {
            return Result<bool>.Failure("Only Draft goods receipts can be deleted.");
        }

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(receipt));

        await lineRepository.SoftDeleteByReceiptIdAsync(id, currentUserService.UserId ?? 0);
        await receiptRepository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);

        await auditLogService.LogAsync("Inventory_GoodsReceipts", id, "SoftDelete", oldValues, null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }

    // First-ever entry point for stock into the system: posts one Receipt ledger entry + balance per line.
    private async Task ApplyStockReceiptAsync(GoodsReceipt receipt)
    {
        var lines = await lineRepository.GetByReceiptIdAsync(receipt.Id);
        foreach (var line in lines)
        {
            var balance = await balanceRepository.FindAsync(line.ItemId, receipt.WarehouseId, line.BatchId);
            if (balance is null)
            {
                var unitOfMeasurementId = await itemDirectoryService.GetUnitOfMeasurementIdAsync(line.ItemId) ?? 0;
                balance = new StockBalance
                {
                    ItemId = line.ItemId,
                    WarehouseId = receipt.WarehouseId,
                    BatchId = line.BatchId,
                    UnitOfMeasurementId = unitOfMeasurementId,
                    QuantityOnHand = line.Quantity,
                    AverageCost = line.UnitCost,
                    CreatedBy = currentUserService.UserId
                };
                await balanceRepository.AddAsync(balance);
            }
            else
            {
                var totalQuantity = balance.QuantityOnHand + line.Quantity;
                balance.AverageCost = totalQuantity == 0
                    ? balance.AverageCost
                    : ((balance.QuantityOnHand * balance.AverageCost) + (line.Quantity * line.UnitCost)) / totalQuantity;
                balance.QuantityOnHand = totalQuantity;
                balance.UpdatedBy = currentUserService.UserId;
                balance.UpdatedAt = DateTime.UtcNow;
                await balanceRepository.UpdateAsync(balance);
            }

            await ledgerRepository.AddAsync(new StockLedger
            {
                ItemId = line.ItemId,
                WarehouseId = receipt.WarehouseId,
                BatchId = line.BatchId,
                MovementType = "Receipt",
                QuantityChange = line.Quantity,
                UnitCost = line.UnitCost,
                ReferenceType = "GoodsReceipt",
                ReferenceId = receipt.Id,
                MovementDate = receipt.ReceiptDate,
                CreatedBy = currentUserService.UserId
            });
        }
    }

    private async Task AddLinesAsync(long receiptId, List<GoodsReceiptLineRequest> lines)
    {
        foreach (var line in lines)
        {
            await lineRepository.AddAsync(new GoodsReceiptLine
            {
                ReceiptId = receiptId,
                ItemId = line.ItemId,
                BatchId = line.BatchId,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                CreatedBy = currentUserService.UserId
            });
        }
    }

    private async Task<GoodsReceiptResponse> ToResponseAsync(GoodsReceipt receipt)
    {
        var lines = await lineRepository.GetByReceiptIdAsync(receipt.Id);
        var lineResponses = lines.Select(l => new GoodsReceiptLineResponse(l.Id, l.ItemId, l.BatchId, l.Quantity, l.UnitCost)).ToList();
        return new GoodsReceiptResponse(
            receipt.Id, receipt.ReceiptNumber, receipt.WarehouseId, receipt.VendorReference, receipt.ReceiptDate,
            receipt.Status, receipt.CurrentApprovalLevel, receipt.Notes, receipt.CreatedAt, lineResponses);
    }
}
