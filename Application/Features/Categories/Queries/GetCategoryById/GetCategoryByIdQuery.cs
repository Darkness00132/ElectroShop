using Application.Constants;
using Application.Features.Categories.Dtos;

namespace Application.Features.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : ICacheableQuery<CategoryDto>
{
    public string CacheKey => $"{CacheNames.Categories}:{Id}";

    public IReadOnlyCollection<string> Tags => [CacheNames.Categories];

    public CacheOptions CacheOptions => new CacheOptions {
        AbsoluteExpiration = TimeSpan.FromDays(30)
    };

    public bool BypassCache => false;
}
