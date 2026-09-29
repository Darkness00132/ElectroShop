using System.ComponentModel.DataAnnotations.Schema;
using Domain.Common;
using Domain.Entities.Catalog;
using Domain.Exceptions;

namespace Domain.Entities.Carts;

public sealed class CartItem : IEntity
{
    public Guid CartId { get; private set; }

    public Guid ProductId { get; private set; }

    public Cart Cart { get; private set; } = null!;

    public Product Product { get; private set; } = null!;

    public int Quantity { get; private set; }

    [NotMapped]
    public decimal LineSubtotal => Product.Price * Quantity;

    [NotMapped]
    public decimal LineDiscount => Product.Discount is { IsActive: true }
        ? Product.Discount.CalculateDiscountAmount(Product.Price) * Quantity
        : 0m;

    [NotMapped]
    public decimal LineTotal => LineSubtotal - LineDiscount;

    private CartItem() { }

    public CartItem(
        Guid cartId,
        Guid productId,
        int quantity)
    {
        if (cartId == Guid.Empty)
            throw new DomainException("Cart ID cannot be empty.");

        if (productId == Guid.Empty)
            throw new DomainException("Product ID cannot be empty.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }

    internal void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        Quantity += quantity;
    }

    internal void ChangeQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        Quantity = quantity;
    }
}
