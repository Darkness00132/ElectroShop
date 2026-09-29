using FluentValidation;

namespace Application.Features.Orders.Commands.PlaceOrder;

internal class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(250);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Notes)
            .MaximumLength(250)
            .When(x => x.Notes is not null);

        RuleFor(x => x.PromoCode)
            .MaximumLength(50)
            .When(x => x.PromoCode is not null);
    }
}
