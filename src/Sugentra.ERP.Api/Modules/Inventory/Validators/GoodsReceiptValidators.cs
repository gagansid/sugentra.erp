using FluentValidation;
using Sugentra.ERP.Api.Modules.Inventory.Dtos;

namespace Sugentra.ERP.Api.Modules.Inventory.Validators;

public class GoodsReceiptLineRequestValidator : AbstractValidator<GoodsReceiptLineRequest>
{
    public GoodsReceiptLineRequestValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.BatchId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
    }
}

public class CreateGoodsReceiptRequestValidator : AbstractValidator<CreateGoodsReceiptRequest>
{
    public CreateGoodsReceiptRequestValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.PurchaseOrderId).GreaterThan(0).When(x => x.PurchaseOrderId.HasValue);
        RuleFor(x => x.ReceiptDate).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line is required.");
        RuleForEach(x => x.Lines).SetValidator(new GoodsReceiptLineRequestValidator());
    }
}

public class UpdateGoodsReceiptRequestValidator : AbstractValidator<UpdateGoodsReceiptRequest>
{
    public UpdateGoodsReceiptRequestValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.PurchaseOrderId).GreaterThan(0).When(x => x.PurchaseOrderId.HasValue);
        RuleFor(x => x.ReceiptDate).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line is required.");
        RuleForEach(x => x.Lines).SetValidator(new GoodsReceiptLineRequestValidator());
    }
}
