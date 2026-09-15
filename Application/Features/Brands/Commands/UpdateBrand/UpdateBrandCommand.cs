using Application.Abstractions;
using Application.Constants;

namespace Application.Features.Brands.Commands.UpdateBrand;

public sealed record UpdateBrandCommand(
    Guid Id,
    string NameEn,
    string NameAr) : ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
        [CacheNames.Brands, $"{CacheNames.Brands}:{Id}"];

    public IReadOnlyCollection<string> CacheTags => [];
}
