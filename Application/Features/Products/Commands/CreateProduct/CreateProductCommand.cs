using Application.Abstractions;
using Application.Common.Files;
using Application.Constants;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string NameEn,
    string NameAr,
    string DescriptionEn,
    string DescriptionAr,
    string SKU,
    decimal Price,
    bool IsActive,
    Guid CategoryId,
    Guid BrandId,
    Guid? DiscountId,
    Dictionary<string, string>? Attributes,
    List<FileDto> Images) : ICacheInvalidatingCommand<Guid>
{
    public IReadOnlyCollection<string> CacheKeys => [];

    public IReadOnlyCollection<string> CacheTags => [CacheNames.Products];
}
