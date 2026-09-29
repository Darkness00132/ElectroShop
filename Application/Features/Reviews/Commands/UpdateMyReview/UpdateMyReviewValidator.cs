using FluentValidation;

namespace Application.Features.Reviews.Commands.UpdateMyReview;

internal class UpdateMyReviewValidator : AbstractValidator<UpdateMyReviewCommand>
{
    public UpdateMyReviewValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Comment));
    }
}
