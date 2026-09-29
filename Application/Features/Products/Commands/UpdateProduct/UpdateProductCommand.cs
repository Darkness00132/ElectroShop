using Application.Abstractions;
using Application.Common.Files;
using Application.Constants;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string? NameEn,
    string? NameAr,
    string? SKU,
    string? DescriptionEn,
    string? DescriptionAr,
    decimal? Price,
    Guid? CategoryId,
    Guid? BrandId,
    List<string>? DeletedImages,
    List<FileDto>? NewImages,
    Dictionary<string, string>? Attributes) : ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheKeys
        => [CacheNames.Products, $"{CacheNames.Products}:{Id}"];

    public IReadOnlyCollection<string> CacheTags => [CacheNames.Products];
}
