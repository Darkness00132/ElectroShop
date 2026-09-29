using FluentValidation;

namespace Application.Features.Brands.Commands.UpdateBrand;

internal class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(command => command.NameEn)
            .MaximumLength(100);

        RuleFor(command => command.NameAr)
            .MaximumLength(100);
    }
}
