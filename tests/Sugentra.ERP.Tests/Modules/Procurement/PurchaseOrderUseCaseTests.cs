using Moq;
using Sugentra.ERP.Api.Modules.Procurement.Dtos;
using Sugentra.ERP.Api.Modules.Procurement.Entities;
using Sugentra.ERP.Api.Modules.Procurement.Repositories;
using Sugentra.ERP.Api.Modules.Procurement.UseCases;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Logging;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Tests.Modules.Procurement;

public class PurchaseOrderUseCaseTests
{
    private readonly Mock<GenericRepository<PurchaseOrder>> orderRepository = new(Mock.Of<IDbConnectionFactory>());
    private readonly Mock<IPurchaseOrderLineRepository> lineRepository = new();
    private readonly Mock<IPurchaseOrderLineSourceRepository> lineSourceRepository = new();
    private readonly Mock<IPurchaseOrderRequisitionRepository> orderRequisitionRepository = new();
    private readonly Mock<GenericRepository<PurchaseRequisition>> requisitionRepository = new(Mock.Of<IDbConnectionFactory>());
    private readonly Mock<IPurchaseRequisitionLineRepository> requisitionLineRepository = new();
    private readonly Mock<IItemDirectoryService> itemDirectoryService = new();
    private readonly Mock<IBusinessPartnerDirectoryService> businessPartnerDirectoryService = new();
    private readonly Mock<IDocumentNumberGeneratorService> documentNumberGeneratorService = new();
    private readonly Mock<IApprovalService> approvalService = new();
    private readonly Mock<IUserDirectoryService> userDirectoryService = new();
    private readonly Mock<IAuditLogService> auditLogService = new();
    private readonly Mock<ICurrentUserService> currentUserService = new();

    private PurchaseOrderUseCase CreateUseCase() => new(
        orderRepository.Object, lineRepository.Object, lineSourceRepository.Object, orderRequisitionRepository.Object, requisitionRepository.Object,
        requisitionLineRepository.Object, itemDirectoryService.Object,
        businessPartnerDirectoryService.Object, documentNumberGeneratorService.Object, approvalService.Object,
        userDirectoryService.Object, auditLogService.Object, currentUserService.Object);

    public PurchaseOrderUseCaseTests()
    {
        currentUserService.SetupGet(x => x.UserId).Returns(1);
        documentNumberGeneratorService.Setup(x => x.GetNextAsync("PurchaseOrder"))
            .ReturnsAsync(new NextDocumentNumberResult(1, "PO/2026/0001"));
        lineRepository.Setup(x => x.GetByOrderIdAsync(It.IsAny<long>()))
            .ReturnsAsync(new List<PurchaseOrderLine>());
        lineSourceRepository.Setup(x => x.GetByOrderLineIdsAsync(It.IsAny<IEnumerable<long>>()))
            .ReturnsAsync(new List<PurchaseOrderLineSource>());
        lineSourceRepository.Setup(x => x.GetOrderedQuantityByRequisitionLineIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<long?>()))
            .ReturnsAsync(new Dictionary<long, decimal>());
        requisitionLineRepository.Setup(x => x.GetByRequisitionIdAsync(It.IsAny<long>()))
            .ReturnsAsync(new List<PurchaseRequisitionLine>());
        orderRequisitionRepository.Setup(x => x.GetByOrderIdAsync(It.IsAny<long>()))
            .ReturnsAsync(new List<PurchaseOrderRequisition>());
        orderRepository.Setup(x => x.AddAsync(It.IsAny<PurchaseOrder>()))
            .Callback<PurchaseOrder>(o => o.Id = 200)
            .ReturnsAsync(200);
    }

    private static CreatePurchaseOrderRequest ValidCreateRequest(long? requisitionId = null) => new(
        PurchaseRequisitionIds: requisitionId is null ? null : [requisitionId.Value], VendorId: 1, CurrencyId: 1, PaymentTermDays: 30,
        OrderDate: DateTime.UtcNow, ExpectedDeliveryDate: null, Notes: null,
        Lines: [new PurchaseOrderLineRequest(ItemId: 1, WarehouseId: 1, Quantity: 10, UnitPrice: 100, DiscountPercent: 0)]);

    [Fact]
    public async Task CreateAsync_DirectWithoutRequisition_Succeeds()
    {
        var useCase = CreateUseCase();

        var result = await useCase.CreateAsync(ValidCreateRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal("Draft", result.Value!.Status);
        Assert.Equal("PO/2026/0001", result.Value.OrderNumber);
    }

    [Fact]
    public async Task CreateAsync_FromNonApprovedRequisition_Fails()
    {
        requisitionRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(new PurchaseRequisition { Id = 5, Status = "Draft" });
        var useCase = CreateUseCase();

        var result = await useCase.CreateAsync(ValidCreateRequest(5));

        Assert.False(result.IsSuccess);
        orderRepository.Verify(x => x.AddAsync(It.IsAny<PurchaseOrder>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_FromApprovedRequisition_Succeeds()
    {
        requisitionRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(new PurchaseRequisition { Id = 5, Status = "Approved", RequisitionNumber = "PR/2026/0005" });
        orderRequisitionRepository.Setup(x => x.GetByOrderIdAsync(200))
            .ReturnsAsync(new List<PurchaseOrderRequisition> { new() { PurchaseOrderId = 200, PurchaseRequisitionId = 5 } });
        var useCase = CreateUseCase();

        var result = await useCase.CreateAsync(ValidCreateRequest(5));

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.SourceRequisitions.Single().Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotDraft_ReturnsFailure()
    {
        orderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new PurchaseOrder { Id = 1, Status = "Approved" });
        var useCase = CreateUseCase();

        var result = await useCase.UpdateAsync(1, new UpdatePurchaseOrderRequest(1, 1, 30, DateTime.UtcNow, null, null, []));

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task SubmitAsync_WhenApprovalNotRequired_FinalizesAsApproved()
    {
        var order = new PurchaseOrder { Id = 1, Status = "Draft", OrderNumber = "PO/2026/0001", VendorId = 1, CurrencyId = 1 };
        orderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        approvalService.Setup(x => x.SubmitForApprovalAsync(It.IsAny<ApprovalSubmissionRequest>()))
            .ReturnsAsync(new ApprovalSubmissionResult(false, null, null));

        var useCase = CreateUseCase();
        var result = await useCase.SubmitAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Approved", result.Value!.Status);
    }

    [Fact]
    public async Task ApplyReceiptAsync_WhenAllLinesFullyReceived_SetsFullyReceivedStatus()
    {
        var order = new PurchaseOrder { Id = 1, Status = "Approved", VendorId = 1, CurrencyId = 1 };
        orderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        lineRepository.Setup(x => x.GetByOrderIdAsync(1))
            .ReturnsAsync(new List<PurchaseOrderLine> { new() { Id = 10, Quantity = 10, ReceivedQuantity = 10 } });

        var useCase = CreateUseCase();
        await useCase.ApplyReceiptAsync(1, [new PurchaseOrderReceiptLineUpdate(10, 10)]);

        lineRepository.Verify(x => x.UpdateReceivedQuantityAsync(10, 10), Times.Once);
        Assert.Equal("FullyReceived", order.Status);
    }

    [Fact]
    public async Task ApplyReceiptAsync_WhenSomeLinesPartial_SetsPartiallyReceivedStatus()
    {
        var order = new PurchaseOrder { Id = 1, Status = "Approved", VendorId = 1, CurrencyId = 1 };
        orderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        lineRepository.Setup(x => x.GetByOrderIdAsync(1))
            .ReturnsAsync(new List<PurchaseOrderLine> { new() { Id = 10, Quantity = 10, ReceivedQuantity = 4 } });

        var useCase = CreateUseCase();
        await useCase.ApplyReceiptAsync(1, [new PurchaseOrderReceiptLineUpdate(10, 4)]);

        Assert.Equal("PartiallyReceived", order.Status);
    }

    [Fact]
    public async Task DeleteAsync_WhenDraft_SoftDeletesHeaderAndLines()
    {
        orderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new PurchaseOrder { Id = 1, Status = "Draft" });
        var useCase = CreateUseCase();

        var result = await useCase.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        lineRepository.Verify(x => x.SoftDeleteByOrderIdAsync(1, 1), Times.Once);
        orderRepository.Verify(x => x.SoftDeleteAsync(1, 1), Times.Once);
    }
}
