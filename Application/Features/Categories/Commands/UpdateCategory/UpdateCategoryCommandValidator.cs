using Application.Common.Validation;
using FluentValidation;

namespace Application.Features.Categories.Commands.UpdateCategory;

internal class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(command => command.NameEn)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.NameAr)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.DescriptionEn)
            .MaximumLength(500);

        RuleFor(command => command.DescriptionAr)
            .MaximumLength(500);

        RuleFor(command => command.NewImage)
            .ValidImageFile(5 * 1024 * 1024)
            .When(command => command.NewImage is not null);
    }
}
