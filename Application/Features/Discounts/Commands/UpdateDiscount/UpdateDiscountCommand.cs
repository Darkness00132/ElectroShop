using Application.Abstractions;
using Application.Constants;
using Domain.Enums;

namespace Application.Features.Discounts.Commands.UpdateDiscount;

public record UpdateDiscountCommand(Guid Id,
    string? Name,
    DiscountType? DiscountType,
    decimal? Value,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool? IsActive) : ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheKeys
        => [CacheNames.Discounts, $"{CacheNames.Discounts}:{Id}"];

    public IReadOnlyCollection<string> CacheTags
        => [CacheNames.Products];
}
