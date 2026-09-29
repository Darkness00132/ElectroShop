using Domain.Entities.Carts;
using Domain.Entities.Catalog;

namespace Application.Common.Checkout;

public sealed record CheckoutLine(
    Guid ProductId,
    string NameEn,
    string NameAr,
    string SKU,
    decimal UnitPrice,
    int Quantity,
    decimal UnitDiscount,
    int AvailableStock)
{
    public decimal LineSubtotal => UnitPrice * Quantity;

    public decimal LineDiscount => UnitDiscount * Quantity;

    public decimal LineTotal => LineSubtotal - LineDiscount;
}

/// <summary>
/// Builds the checkout lines shared by the checkout summary and order placement so that
/// both always compute prices, discounts, and stock from the same source.
/// </summary>
internal static class CheckoutCalculator
{
    public static IReadOnlyList<CheckoutLine> BuildLines(
        IReadOnlyCollection<CartItem> items,
        IReadOnlyCollection<Product> products,
        IReadOnlyDictionary<Guid, int> stockByProduct)
    {
        return items.Select(item => {
            var product = products.Single(p => p.Id == item.ProductId);

            var unitDiscount = product.Discount is { IsActive: true }
                ? product.Discount.CalculateDiscountAmount(product.Price)
                : 0m;

            return new CheckoutLine(
                product.Id,
                product.NameEn,
                product.NameAr,
                product.SKU,
                product.Price,
                item.Quantity,
                unitDiscount,
                stockByProduct.GetValueOrDefault(product.Id));
        }).ToList();
    }
}
