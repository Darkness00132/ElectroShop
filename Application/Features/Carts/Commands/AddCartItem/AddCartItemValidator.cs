using FluentValidation;

namespace Application.Features.Carts.Commands.AddCartItem;

internal class AddCartItemValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
