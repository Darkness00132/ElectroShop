using Application.Abstractions;
using Application.Constants;
using MediatR;

namespace Application.Features.Products.Commands.ChangeProductPrice;

public sealed record ChangeProductPriceCommand(
    Guid ProductId,
    decimal Price) : ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheKeys
        => [CacheNames.Products, $"{CacheNames.Products}:{ProductId}"];

    public IReadOnlyCollection<string> CacheTags => [CacheNames.Products];
}
