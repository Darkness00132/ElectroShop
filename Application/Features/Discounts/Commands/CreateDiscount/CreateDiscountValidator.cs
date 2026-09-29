using Application.Features.Discounts.Commands.CreateDiscount;
using Domain.Enums;
using FluentValidation;

public sealed class CreateDiscountCommandValidator
    : AbstractValidator<CreateDiscountCommand>
{
    public CreateDiscountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Value)
            .GreaterThan(0);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate);

        RuleFor(x => x.Value)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == DiscountType.Percentage);
    }
}
