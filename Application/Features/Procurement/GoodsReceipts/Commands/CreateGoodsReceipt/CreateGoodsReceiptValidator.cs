using FluentValidation;

namespace Application.Features.Procurement.GoodsReceipts.Commands.CreateGoodsReceipt;

internal class CreateGoodsReceiptValidator : AbstractValidator<CreateGoodsReceiptCommand>
{
    public CreateGoodsReceiptValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty();

        RuleFor(x => x.DeliveryReference)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.DeliveryReference));

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
