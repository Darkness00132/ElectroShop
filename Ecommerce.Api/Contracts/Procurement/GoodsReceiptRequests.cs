namespace Ecommerce.Api.Contracts.Procurement;

/// <summary>
/// Represents the request body for creating a goods receipt in draft state.
/// </summary>
/// <param name="Number">The unique goods receipt number.</param>
/// <param name="PurchaseOrderId">The purchase order being received.</param>
/// <param name="ReceivedAt">When the goods were received.</param>
/// <param name="DeliveryReference">The optional delivery reference from the carrier.</param>
/// <param name="Notes">Optional notes.</param>
public sealed record CreateGoodsReceiptRequest(
    string Number,
    Guid PurchaseOrderId,
    DateTime ReceivedAt,
    string? DeliveryReference,
    string? Notes);

/// <summary>
/// Represents the request body for adding a received product line to a draft goods receipt.
/// </summary>
/// <param name="ProductId">The product that was received.</param>
/// <param name="Quantity">The received quantity.</param>
public sealed record AddGoodsReceiptItemRequest(Guid ProductId, int Quantity);
