using Application.Abstractions;
using Application.Constants;
using MediatR;

namespace Application.Features.Products.Commands.RemoveProductDiscount;

public sealed record RemoveProductDiscountCommand(Guid ProductId) : ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheKeys
        => [CacheNames.Products, $"{CacheNames.Products}:{ProductId}"];

    public IReadOnlyCollection<string> CacheTags => [CacheNames.Products];
}
