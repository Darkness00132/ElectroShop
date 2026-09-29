using Application.Common.Validation;
using FluentValidation;

namespace Application.Features.Products.Commands.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.BrandId)
            .NotEmpty();

        RuleFor(x => x.Images)
            .ForEach(x => x.ValidImageFile(10 * 1024 * 1024));

        RuleFor(x => x.Images.Count)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(5); // Product gallery limit
    }
}
