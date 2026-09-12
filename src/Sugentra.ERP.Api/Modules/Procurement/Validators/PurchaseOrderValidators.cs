using FluentValidation;
using Sugentra.ERP.Api.Modules.Procurement.Dtos;

namespace Sugentra.ERP.Api.Modules.Procurement.Validators;

public class PurchaseOrderLineRequestValidator : AbstractValidator<PurchaseOrderLineRequest>
{
    public PurchaseOrderLineRequestValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100);
    }
}

public class CreatePurchaseOrderRequestValidator : AbstractValidator<CreatePurchaseOrderRequest>
{
    public CreatePurchaseOrderRequestValidator()
    {
        RuleFor(x => x.VendorId).GreaterThan(0);
        RuleFor(x => x.CurrencyId).GreaterThan(0);
        RuleFor(x => x.OrderDate).NotEmpty();
        RuleFor(x => x.PaymentTermDays).GreaterThanOrEqualTo(0).When(x => x.PaymentTermDays.HasValue);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line is required.");
        RuleForEach(x => x.Lines).SetValidator(new PurchaseOrderLineRequestValidator());
    }
}

public class UpdatePurchaseOrderRequestValidator : AbstractValidator<UpdatePurchaseOrderRequest>
{
    public UpdatePurchaseOrderRequestValidator()
    {
        RuleFor(x => x.VendorId).GreaterThan(0);
        RuleFor(x => x.CurrencyId).GreaterThan(0);
        RuleFor(x => x.OrderDate).NotEmpty();
        RuleFor(x => x.PaymentTermDays).GreaterThanOrEqualTo(0).When(x => x.PaymentTermDays.HasValue);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line is required.");
        RuleForEach(x => x.Lines).SetValidator(new PurchaseOrderLineRequestValidator());
    }
}
