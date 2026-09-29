using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Checkout;
using Application.Exceptions;
using Application.Settings;
using Domain.Entities.Carts;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.OrdersAggregate;
using Domain.Entities.PromotionsAggregate;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Features.Orders.Commands.PlaceOrder;

internal class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, Guid>
{
    private readonly IRepository<Cart> _cartRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly IRepository<PromoCode> _promoCodeRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IOptions<ShippingSettings> _shippingOptions;

    public PlaceOrderHandler(IRepository<Cart> cartRepository, IRepository<Product> productRepository, IRepository<Inventory> inventoryRepository, IRepository<PromoCode> promoCodeRepository, IRepository<Order> orderRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUser, IOptions<ShippingSettings> shippingOptions)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _promoCodeRepository = promoCodeRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _shippingOptions = shippingOptions;
    }

    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var cart = await _cartRepository.SingleOrDefaultAsync(
            c => c.UserId == userId,
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

        ValidateLines(products, lines);

        var order = new Order(userId, new Address(request.Street, request.City, request.Phone, request.Notes));

        foreach (var line in lines)
            order.AddItem(line.ProductId, line.NameEn, line.NameAr, line.SKU, line.Quantity, line.UnitPrice, line.UnitDiscount * line.Quantity);

        order.SetShippingFee(_shippingOptions.Value.Fee);

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
            await ApplyPromoCodeAsync(order, request.PromoCode, lines.Sum(line => line.LineTotal), cancellationToken);

        foreach (var line in lines)
            inventories.Single(i => i.ProductId == line.ProductId).DecreaseStock(line.Quantity, order.Id);

        cart.Clear();

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    private static void ValidateLines(IReadOnlyCollection<Product> products, IReadOnlyList<CheckoutLine> lines)
    {
        foreach (var line in lines) {
            var product = products.Single(p => p.Id == line.ProductId);

            if (!product.IsActive)
                throw new ConflictException($"The product '{product.NameEn}' is not available.");

            if (line.AvailableStock < line.Quantity)
                throw new ConflictException($"Insufficient stock for '{product.NameEn}'.");
        }
    }

    private async Task ApplyPromoCodeAsync(Order order, string promoCode, decimal itemsNetTotal, CancellationToken cancellationToken)
    {
        var normalizedCode = promoCode.Trim().ToUpperInvariant();

        var promo = await _promoCodeRepository.SingleOrDefaultAsync(p => p.Code == normalizedCode, cancellationToken)
            ?? throw new ConflictException("The promo code is invalid.");

        if (!promo.CanBeUsed(itemsNetTotal, DateOnly.FromDateTime(DateTime.UtcNow)))
            throw new ConflictException("The promo code cannot be used.");

        order.ApplyPromoCode(promo.Id, promo.CalculateDiscount(itemsNetTotal));

        promo.MarkAsUsed();
    }
}
