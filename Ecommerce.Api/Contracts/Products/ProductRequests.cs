namespace Ecommerce.Api.Contracts.Products;

/// <summary>
/// Represents the request body for changing the price of a product.
/// </summary>
/// <param name="Price">The new price of the product.</param>
public sealed record UpdateProductPriceRequest(decimal Price);

/// <summary>
/// Represents the request body for assigning a discount to a product.
/// </summary>
/// <param name="DiscountId">The identifier of the discount to assign.</param>
public sealed record AssignProductDiscountRequest(Guid DiscountId);
