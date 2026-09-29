using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Checkout;
using Application.Exceptions;
using Application.Settings;
using Domain.Entities.Carts;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Features.Carts.Queries.GetCheckoutSummary;

internal class GetCheckoutSummaryHandler : IRequestHandler<GetCheckoutSummaryQuery, CheckoutSummaryDto>
{
    private readonly IRepository<Cart> _cartRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IOptions<ShippingSettings> _shippingOptions;

    public GetCheckoutSummaryHandler(IRepository<Cart> cartRepository, IRepository<Product> productRepository, IRepository<Inventory> inventoryRepository, ICurrentUserService currentUser, IOptions<ShippingSettings> shippingOptions)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _currentUser = currentUser;
        _shippingOptions = shippingOptions;
    }

    public async Task<CheckoutSummaryDto> Handle(GetCheckoutSummaryQuery request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.SingleOrDefaultAsync(
            c => c.UserId == _currentUser.UserId,
            cancellationToken,
            c => c.Items)
            ?? throw new ConflictException("Your cart is empty.");

        if (cart.Items.Count == 0)
            throw new ConflictException("Your cart is empty.");

        var productIds = cart.Items.Select(item => item.ProductId).ToList();

        var products = await _productRepository.ListAsync(
            p => productIds.Contains(p.Id),
            cancellationToken,
            p => p.Discount);

        var inventories = await _inventoryRepository.ListAsync(
            i => productIds.Contains(i.ProductId),
            cancellationToken);

        var stockByProduct = inventories.ToDictionary(i => i.ProductId, i => i.QuantityOnHand);

        var lines = CheckoutCalculator.BuildLines(cart.Items, products, stockByProduct);

        var subtotal = lines.Sum(line => line.LineSubtotal);
        var itemsDiscountAmount = lines.Sum(line => line.LineDiscount);
        var shippingFee = _shippingOptions.Value.Fee;

        return new CheckoutSummaryDto(lines, subtotal, itemsDiscountAmount, shippingFee, subtotal - itemsDiscountAmount + shippingFee);
    }
}
