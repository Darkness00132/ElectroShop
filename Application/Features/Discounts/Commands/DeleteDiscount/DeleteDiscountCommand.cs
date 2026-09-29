using Application.Abstractions;
using Application.Constants;

namespace Application.Features.Discounts.Commands.DeleteDiscount;

public record DeleteDiscountCommand(Guid Id) : ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheKeys
        => [CacheNames.Discounts, $"{CacheNames.Discounts}:{Id}"];

    public IReadOnlyCollection<string> CacheTags
        => [CacheNames.Products];
}
