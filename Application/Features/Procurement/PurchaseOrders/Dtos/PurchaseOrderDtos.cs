using Domain.Enums;

namespace Application.Features.Procurement.PurchaseOrders.Dtos;

public sealed record PurchaseOrderDto(
    Guid Id,
    string Number,
    Guid SupplierId,
    string SupplierName,
    PurchaseOrderStatus Status,
    DateOnly OrderDate,
    DateOnly? ExpectedDeliveryDate,
    decimal Subtotal,
    decimal TaxAmount,
    decimal ShippingCost,
    decimal Total,
    string? Notes,
    DateTime CreatedAt,
    IReadOnlyList<PurchaseOrderItemDto> Items);

public sealed record PurchaseOrderItemDto(
    Guid ProductId,
    string ProductNameEn,
    int OrderedQuantity,
    int ReceivedQuantity,
    decimal UnitCost);
