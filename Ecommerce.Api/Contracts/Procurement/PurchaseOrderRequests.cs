namespace Ecommerce.Api.Contracts.Procurement;

/// <summary>
/// Represents the request body for creating a purchase order in draft state.
/// </summary>
/// <param name="Number">The unique purchase order number.</param>
/// <param name="SupplierId">The supplier to order from.</param>
/// <param name="OrderDate">The date the order is placed.</param>
/// <param name="ExpectedDeliveryDate">The optional expected delivery date; cannot be before the order date.</param>
/// <param name="Notes">Optional notes.</param>
public sealed record CreatePurchaseOrderRequest(
    string Number,
    Guid SupplierId,
    DateOnly OrderDate,
    DateOnly? ExpectedDeliveryDate,
    string? Notes);

/// <summary>
/// Represents the request body for adding a product line to a draft purchase order.
/// </summary>
/// <param name="ProductId">The product to order.</param>
/// <param name="OrderedQuantity">The quantity to order.</param>
/// <param name="UnitCost">The cost per unit.</param>
public sealed record AddPurchaseOrderItemRequest(Guid ProductId, int OrderedQuantity, decimal UnitCost);

/// <summary>
/// Represents the request body for setting the tax and shipping costs of a purchase order.
/// </summary>
/// <param name="TaxAmount">The tax amount.</param>
/// <param name="ShippingCost">The shipping cost.</param>
public sealed record SetPurchaseOrderCostsRequest(decimal TaxAmount, decimal ShippingCost);
