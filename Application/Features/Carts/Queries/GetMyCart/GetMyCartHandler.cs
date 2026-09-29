using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Features.Carts.Dtos;
using Domain.Entities.Carts;
using Domain.Entities.Catalog;
using MediatR;

namespace Application.Features.Carts.Queries.GetMyCart;

internal class GetMyCartHandler : IRequestHandler<GetMyCartQuery, CartDto>
{
    private readonly IRepository<Cart> _cartRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly ICurrentUserService _currentUser;

    public GetMyCartHandler(IRepository<Cart> cartRepository, IRepository<Product> productRepository, ICurrentUserService currentUser)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _currentUser = currentUser;
    }

    public async Task<CartDto> Handle(GetMyCartQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var cart = await _cartRepository.SingleOrDefaultAsync(
            c => c.UserId == userId,
            cancellationToken,
            c => c.Items);

        if (cart is null || cart.Items.Count == 0)
            return new CartDto(userId, [], 0m, 0m, 0m);

        var productIds = cart.Items.Select(item => item.ProductId).ToList();

        var products = await _productRepository.ListAsync(
            p => productIds.Contains(p.Id),
            cancellationToken,
            p => p.Discount);

        var items = cart.Items.Select(item => {
            var product = products.Single(p => p.Id == item.ProductId);

            var lineSubtotal = product.Price * item.Quantity;

            var lineDiscount = product.Discount is { IsActive: true }
                ? product.Discount.CalculateDiscountAmount(product.Price) * item.Quantity
                : 0m;

            return new CartItemDto(
                product.Id,
                product.NameEn,
                product.NameAr,
                product.Price,
                item.Quantity,
                lineSubtotal,
                lineDiscount,
                lineSubtotal - lineDiscount);
        }).ToList();

        var subtotal = items.Sum(item => item.LineSubtotal);
        var discountAmount = items.Sum(item => item.LineDiscount);

        return new CartDto(userId, items, subtotal, discountAmount, subtotal - discountAmount);
    }
}
