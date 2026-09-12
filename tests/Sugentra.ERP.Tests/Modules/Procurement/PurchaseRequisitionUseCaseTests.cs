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

public class PurchaseRequisitionUseCaseTests
{
    private readonly Mock<GenericRepository<PurchaseRequisition>> requisitionRepository = new(Mock.Of<IDbConnectionFactory>());
    private readonly Mock<IPurchaseRequisitionLineRepository> lineRepository = new();
    private readonly Mock<IPurchaseOrderRequisitionRepository> orderRequisitionRepository = new();
    private readonly Mock<GenericRepository<PurchaseOrder>> orderRepository = new(Mock.Of<IDbConnectionFactory>());
    private readonly Mock<IPurchaseOrderLineSourceRepository> lineSourceRepository = new();
    private readonly Mock<IItemDirectoryService> itemDirectoryService = new();
    private readonly Mock<IDocumentNumberGeneratorService> documentNumberGeneratorService = new();
    private readonly Mock<IApprovalService> approvalService = new();
    private readonly Mock<IUserDirectoryService> userDirectoryService = new();
    private readonly Mock<IAuditLogService> auditLogService = new();
    private readonly Mock<ICurrentUserService> currentUserService = new();

    private PurchaseRequisitionUseCase CreateUseCase() => new(
        requisitionRepository.Object, lineRepository.Object, orderRequisitionRepository.Object, orderRepository.Object, lineSourceRepository.Object,
        itemDirectoryService.Object, documentNumberGeneratorService.Object, approvalService.Object, userDirectoryService.Object,
        auditLogService.Object, currentUserService.Object);

    public PurchaseRequisitionUseCaseTests()
    {
        currentUserService.SetupGet(x => x.UserId).Returns(1);
        documentNumberGeneratorService.Setup(x => x.GetNextAsync("PurchaseRequisition"))
            .ReturnsAsync(new NextDocumentNumberResult(1, "PR/2026/0001"));
        lineRepository.Setup(x => x.GetByRequisitionIdAsync(It.IsAny<long>()))
            .ReturnsAsync(new List<PurchaseRequisitionLine>());
        orderRequisitionRepository.Setup(x => x.GetOrderIdsByRequisitionIdsAsync(It.IsAny<IEnumerable<long>>()))
            .ReturnsAsync(new Dictionary<long, IReadOnlyList<long>>());
        lineSourceRepository.Setup(x => x.GetOrderedQuantityByRequisitionIdsAsync(It.IsAny<IEnumerable<long>>()))
            .ReturnsAsync(new Dictionary<long, decimal>());
        requisitionRepository.Setup(x => x.AddAsync(It.IsAny<PurchaseRequisition>()))
            .Callback<PurchaseRequisition>(r => r.Id = 100)
            .ReturnsAsync(100);
    }

    [Fact]
    public async Task CreateAsync_SetsDraftStatusAndGeneratesNumber()
    {
        var useCase = CreateUseCase();
        var request = new CreatePurchaseRequisitionRequest(
            WarehouseId: 5, RequisitionDate: DateTime.UtcNow, Notes: "test",
            Lines: [new PurchaseRequisitionLineRequest(ItemId: 1, Quantity: 10, Notes: null)]);

        var response = await useCase.CreateAsync(request);

        Assert.Equal("Draft", response.Status);
        Assert.Equal("PR/2026/0001", response.RequisitionNumber);
        lineRepository.Verify(x => x.AddAsync(It.Is<PurchaseRequisitionLine>(l => l.ItemId == 1 && l.Quantity == 10)), Times.Once);
        auditLogService.Verify(x => x.LogAsync("Procurement_PurchaseRequisitions", 100, "Create", null, It.IsAny<string>(), 1), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotDraft_ReturnsFailure()
    {
        requisitionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new PurchaseRequisition { Id = 1, Status = "WaitingApproval" });
        var useCase = CreateUseCase();

        var result = await useCase.UpdateAsync(1, new UpdatePurchaseRequisitionRequest(5, DateTime.UtcNow, null, []));

        Assert.False(result.IsSuccess);
        Assert.Contains("Draft", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WhenDraft_ReplacesLinesAndUpdates()
    {
        var requisition = new PurchaseRequisition { Id = 1, Status = "Draft", RequesterUserId = 1 };
        requisitionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(requisition);
        var useCase = CreateUseCase();

        var result = await useCase.UpdateAsync(1, new UpdatePurchaseRequisitionRequest(
            9, DateTime.UtcNow, "updated", [new PurchaseRequisitionLineRequest(2, 5, null)]));

        Assert.True(result.IsSuccess);
        Assert.Equal(9, requisition.WarehouseId);
        lineRepository.Verify(x => x.SoftDeleteByRequisitionIdAsync(1, It.IsAny<long>()), Times.Once);
        lineRepository.Verify(x => x.AddAsync(It.Is<PurchaseRequisitionLine>(l => l.ItemId == 2)), Times.Once);
        requisitionRepository.Verify(x => x.UpdateAsync(requisition), Times.Once);
    }

    [Fact]
    public async Task SubmitAsync_WhenApprovalRequired_SetsWaitingApprovalAndDoesNotFinalize()
    {
        var requisition = new PurchaseRequisition { Id = 1, Status = "Draft", RequisitionNumber = "PR/2026/0001", RequesterUserId = 1 };
        requisitionRepository.SetupSequence(x => x.GetByIdAsync(1))
            .ReturnsAsync(requisition)
            .ReturnsAsync(new PurchaseRequisition { Id = 1, Status = "WaitingApproval", CurrentApprovalLevel = "Level 1", RequesterUserId = 1 });
        approvalService.Setup(x => x.SubmitForApprovalAsync(It.IsAny<ApprovalSubmissionRequest>()))
            .ReturnsAsync(new ApprovalSubmissionResult(true, 10, null));

        var useCase = CreateUseCase();
        var result = await useCase.SubmitAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("WaitingApproval", result.Value!.Status);
        requisitionRepository.Verify(x => x.UpdateAsync(It.Is<PurchaseRequisition>(r => r.Status == "Approved")), Times.Never);
    }

    [Fact]
    public async Task SubmitAsync_WhenApprovalNotRequired_FinalizesAsApproved()
    {
        var requisition = new PurchaseRequisition { Id = 1, Status = "Draft", RequisitionNumber = "PR/2026/0001", RequesterUserId = 1 };
        requisitionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(requisition);
        approvalService.Setup(x => x.SubmitForApprovalAsync(It.IsAny<ApprovalSubmissionRequest>()))
            .ReturnsAsync(new ApprovalSubmissionResult(false, null, null));

        var useCase = CreateUseCase();
        var result = await useCase.SubmitAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Approved", result.Value!.Status);
        auditLogService.Verify(x => x.LogAsync("Procurement_PurchaseRequisitions", 1, "Approved", It.IsAny<string>(), It.IsAny<string>(), 1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotDraft_ReturnsFailure()
    {
        requisitionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new PurchaseRequisition { Id = 1, Status = "Approved" });
        var useCase = CreateUseCase();

        var result = await useCase.DeleteAsync(1);

        Assert.False(result.IsSuccess);
        requisitionRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenDraft_SoftDeletesHeaderAndLines()
    {
        requisitionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new PurchaseRequisition { Id = 1, Status = "Draft" });
        var useCase = CreateUseCase();

        var result = await useCase.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        lineRepository.Verify(x => x.SoftDeleteByRequisitionIdAsync(1, 1), Times.Once);
        requisitionRepository.Verify(x => x.SoftDeleteAsync(1, 1), Times.Once);
    }

    [Fact]
    public async Task RejectAsync_WhenWaitingApproval_RevertsToDraft()
    {
        var requisition = new PurchaseRequisition { Id = 1, Status = "WaitingApproval", CurrentApprovalLevel = "Level 1" };
        requisitionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(requisition);
        var useCase = CreateUseCase();

        await useCase.RejectAsync(1);

        Assert.Equal("Draft", requisition.Status);
        Assert.Null(requisition.CurrentApprovalLevel);
    }
}
