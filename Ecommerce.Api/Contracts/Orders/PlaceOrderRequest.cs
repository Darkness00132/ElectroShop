namespace Ecommerce.Api.Contracts.Orders;

/// <summary>
/// Represents the request body for placing an order from the customer's cart.
/// </summary>
/// <param name="Street">The street of the shipping address.</param>
/// <param name="City">The city of the shipping address.</param>
/// <param name="Phone">The contact phone for the delivery.</param>
/// <param name="Notes">Optional delivery notes.</param>
/// <param name="PromoCode">An optional promo code to apply to the order.</param>
public sealed record PlaceOrderRequest(
    string Street,
    string City,
    string Phone,
    string? Notes,
    string? PromoCode);
