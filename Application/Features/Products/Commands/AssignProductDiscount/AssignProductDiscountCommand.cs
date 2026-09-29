using Application.Abstractions;
using Application.Constants;
using MediatR;

namespace Application.Features.Products.Commands.AssignProductDiscount;

public sealed record AssignProductDiscountCommand(
    Guid ProductId,
    Guid DiscountId) : ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheKeys
        => [CacheNames.Products, $"{CacheNames.Products}:{ProductId}"];

    public IReadOnlyCollection<string> CacheTags => [CacheNames.Products];
}
