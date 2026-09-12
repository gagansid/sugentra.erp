using FluentValidation;
using Sugentra.ERP.Api.Modules.Procurement.Dtos;

namespace Sugentra.ERP.Api.Modules.Procurement.Validators;

public class PurchaseRequisitionLineRequestValidator : AbstractValidator<PurchaseRequisitionLineRequest>
{
    public PurchaseRequisitionLineRequestValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class CreatePurchaseRequisitionRequestValidator : AbstractValidator<CreatePurchaseRequisitionRequest>
{
    public CreatePurchaseRequisitionRequestValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.RequisitionDate).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line is required.");
        RuleForEach(x => x.Lines).SetValidator(new PurchaseRequisitionLineRequestValidator());
    }
}

public class UpdatePurchaseRequisitionRequestValidator : AbstractValidator<UpdatePurchaseRequisitionRequest>
{
    public UpdatePurchaseRequisitionRequestValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.RequisitionDate).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line is required.");
        RuleForEach(x => x.Lines).SetValidator(new PurchaseRequisitionLineRequestValidator());
    }
}
