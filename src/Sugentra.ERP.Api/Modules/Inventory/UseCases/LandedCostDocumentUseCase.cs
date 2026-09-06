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

// Landed cost (freight/insurance/handling/duty) is a physical stock-valuation concern, not a Finance/GL
// concern — it directly raises AverageCost on the batches it was received against (see docs/modules/inventory.md).
public class LandedCostDocumentUseCase(
    GenericRepository<LandedCostDocument> documentRepository,
    ILandedCostDocumentLineRepository lineRepository,
    GenericRepository<LandedCostAllocation> allocationRepository,
    GenericRepository<GoodsReceipt> receiptRepository,
    IGoodsReceiptLineRepository receiptLineRepository,
    IStockBalanceRepository balanceRepository,
    IDocumentNumberGeneratorService documentNumberGeneratorService,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    public async Task<IReadOnlyList<LandedCostDocumentResponse>> GetAllAsync()
    {
        var documents = await documentRepository.GetAllAsync();
        return await Task.WhenAll(documents.Select(ToResponseAsync));
    }

    public async Task<LandedCostDocumentResponse?> GetByIdAsync(long id)
    {
        var document = await documentRepository.GetByIdAsync(id);
        return document is null ? null : await ToResponseAsync(document);
    }

    public async Task<Result<LandedCostDocumentResponse>> CreateAsync(CreateLandedCostDocumentRequest request)
    {
        if (request.Lines.Count == 0)
        {
            return Result<LandedCostDocumentResponse>.Failure("At least one cost line is required.");
        }

        var receipt = await receiptRepository.GetByIdAsync(request.GoodsReceiptId);
        if (receipt is null)
        {
            return Result<LandedCostDocumentResponse>.Failure($"GoodsReceipt {request.GoodsReceiptId} not found.");
        }

        var number = await documentNumberGeneratorService.GetNextAsync("LandedCost");

        var document = new LandedCostDocument
        {
            DocumentNumber = number.FormattedNumber,
            GoodsReceiptId = request.GoodsReceiptId,
            AllocationMethod = request.AllocationMethod,
            Status = "Draft",
            Notes = request.Notes,
            CreatedBy = currentUserService.UserId
        };

        var id = await documentRepository.AddAsync(document);
        foreach (var line in request.Lines)
        {
            await lineRepository.AddAsync(new LandedCostDocumentLine
            {
                LandedCostDocumentId = id,
                CostType = line.CostType,
                Amount = line.Amount,
                CurrencyId = line.CurrencyId,
                Notes = line.Notes,
                CreatedBy = currentUserService.UserId
            });
        }

        var response = await ToResponseAsync(document);
        await auditLogService.LogAsync("Inventory_LandedCostDocuments", id, "Create", null, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<LandedCostDocumentResponse>.Success(response);
    }

    // Allocates each cost line's total, proportional to (Quantity*UnitCost) per receipt line, then raises
    // AverageCost on the matching StockBalance — quantity is unaffected, only its unit cost increases.
    public async Task<Result<LandedCostDocumentResponse>> PostAsync(long id)
    {
        var document = await documentRepository.GetByIdAsync(id);
        if (document is null)
        {
            return Result<LandedCostDocumentResponse>.Failure($"LandedCostDocument {id} not found.");
        }

        if (document.Status != "Draft")
        {
            return Result<LandedCostDocumentResponse>.Failure("Only Draft landed cost documents can be posted.");
        }

        var receipt = await receiptRepository.GetByIdAsync(document.GoodsReceiptId);
        if (receipt is null)
        {
            return Result<LandedCostDocumentResponse>.Failure($"GoodsReceipt {document.GoodsReceiptId} not found.");
        }

        var receiptLines = await receiptLineRepository.GetByReceiptIdAsync(document.GoodsReceiptId);
        var totalValue = receiptLines.Sum(l => l.Quantity * l.UnitCost);
        if (receiptLines.Count == 0 || totalValue <= 0)
        {
            return Result<LandedCostDocumentResponse>.Failure("The referenced goods receipt has no valued lines to allocate cost to.");
        }

        var costLines = await lineRepository.GetByDocumentIdAsync(id);
        if (costLines.Count == 0)
        {
            return Result<LandedCostDocumentResponse>.Failure("Add at least one cost line before posting.");
        }

        var totalCost = costLines.Sum(l => l.Amount);
        var currencyId = costLines[0].CurrencyId;

        var oldValues = JsonSerializer.Serialize(await ToResponseAsync(document));

        foreach (var line in receiptLines)
        {
            var share = (line.Quantity * line.UnitCost) / totalValue;
            var allocatedAmount = totalCost * share;

            await allocationRepository.AddAsync(new LandedCostAllocation
            {
                BatchId = line.BatchId,
                LandedCostDocumentId = id,
                CostType = "Other",
                Amount = allocatedAmount,
                CurrencyId = currencyId,
                Notes = $"Auto-allocated from {document.DocumentNumber}",
                CreatedBy = currentUserService.UserId
            });

            var balance = await balanceRepository.FindAsync(line.ItemId, receipt.WarehouseId, line.BatchId);
            if (balance is not null && balance.QuantityOnHand > 0)
            {
                balance.AverageCost += allocatedAmount / balance.QuantityOnHand;
                balance.UpdatedBy = currentUserService.UserId;
                balance.UpdatedAt = DateTime.UtcNow;
                await balanceRepository.UpdateAsync(balance);
            }
        }

        document.Status = "Posted";
        document.UpdatedBy = currentUserService.UserId;
        document.UpdatedAt = DateTime.UtcNow;
        await documentRepository.UpdateAsync(document);

        var response = await ToResponseAsync(document);
        await auditLogService.LogAsync("Inventory_LandedCostDocuments", id, "Posted", oldValues, JsonSerializer.Serialize(response), currentUserService.UserId);

        return Result<LandedCostDocumentResponse>.Success(response);
    }

    private async Task<LandedCostDocumentResponse> ToResponseAsync(LandedCostDocument document)
    {
        var lines = await lineRepository.GetByDocumentIdAsync(document.Id);
        var allocations = (await allocationRepository.GetAllAsync())
            .Where(a => a.LandedCostDocumentId == document.Id)
            .ToList();

        return new LandedCostDocumentResponse(
            document.Id, document.DocumentNumber, document.GoodsReceiptId, document.AllocationMethod, document.Status, document.Notes,
            document.CreatedAt,
            lines.Select(l => new LandedCostDocumentLineResponse(l.Id, l.CostType, l.Amount, l.CurrencyId, l.Notes)).ToList(),
            allocations.Select(a => new LandedCostAllocationResultResponse(a.Id, a.BatchId, a.Amount, a.CurrencyId)).ToList());
    }
}
