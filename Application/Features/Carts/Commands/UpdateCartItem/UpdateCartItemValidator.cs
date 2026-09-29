using FluentValidation;

namespace Application.Features.Carts.Commands.UpdateCartItem;

internal class UpdateCartItemValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
