namespace Ecommerce.Api.Contracts.Carts;

/// <summary>
/// Represents the request body for adding a product to the cart.
/// </summary>
/// <param name="ProductId">The identifier of the product to add.</param>
/// <param name="Quantity">The quantity to add; adding to an existing line increases its quantity.</param>
public sealed record AddCartItemRequest(Guid ProductId, int Quantity);

/// <summary>
/// Represents the request body for changing the quantity of a cart item.
/// </summary>
/// <param name="Quantity">The new quantity of the cart line.</param>
public sealed record UpdateCartItemRequest(int Quantity);
