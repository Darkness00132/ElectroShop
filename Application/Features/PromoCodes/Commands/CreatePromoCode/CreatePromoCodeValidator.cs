using FluentValidation;

namespace Application.Features.PromoCodes.Commands.CreatePromoCode;

internal class CreatePromoCodeValidator : AbstractValidator<CreatePromoCodeCommand>
{
    public CreatePromoCodeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Value)
            .GreaterThan(0);

        RuleFor(x => x.Value)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == Domain.Enums.PromoDiscountType.Percentage);

        RuleFor(x => x.MinimumOrder)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate);

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .When(x => x.UsageLimit.HasValue);
    }
}
