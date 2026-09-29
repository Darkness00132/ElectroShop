using FluentValidation;

namespace Application.Features.PromoCodes.Commands.UpdatePromoCode;

internal class UpdatePromoCodeValidator : AbstractValidator<UpdatePromoCodeCommand>
{
    public UpdatePromoCodeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .When(x => x.Code is not null);

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .When(x => x.Value.HasValue);

        RuleFor(x => x.Value)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == Domain.Enums.PromoDiscountType.Percentage && x.Value.HasValue);

        RuleFor(x => x.MinimumOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinimumOrder.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.EndDate.HasValue && x.StartDate.HasValue);

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .When(x => x.UsageLimit.HasValue);
    }
}
