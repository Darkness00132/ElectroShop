using Application.Abstractions;
using Application.Common.Files;
using Application.Constants;

namespace Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string NameEn,
    string NameAr,
    string DescriptionEn,
    string DescriptionAr,
    FileDto? NewImage) : ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheKeys
        => [CacheNames.Categories, $"{CacheNames.Categories}:{Id}"];

    public IReadOnlyCollection<string> CacheTags => [];
}
