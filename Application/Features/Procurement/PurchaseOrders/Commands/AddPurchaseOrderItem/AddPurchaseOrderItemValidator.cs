using FluentValidation;

namespace Application.Features.Procurement.PurchaseOrders.Commands.AddPurchaseOrderItem;

internal class AddPurchaseOrderItemValidator : AbstractValidator<AddPurchaseOrderItemCommand>
{
    public AddPurchaseOrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.OrderedQuantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0);
    }
}
