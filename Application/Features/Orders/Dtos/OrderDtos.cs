using Domain.Enums;

namespace Application.Features.Orders.Dtos;

public sealed record OrderSummaryDto(
    Guid Id,
    OrderStatus Status,
    decimal Total,
    DateTime CreatedAt,
    int ItemsCount);

public sealed record OrderDto(
    Guid Id,
    OrderStatus Status,
    decimal Subtotal,
    decimal ShippingFee,
    decimal ItemsDiscountAmount,
    decimal PromoDiscountAmount,
    decimal Total,
    DateTime CreatedAt,
    OrderAddressDto ShippingAddress,
    IReadOnlyList<OrderItemDto> Items);

public sealed record OrderAddressDto(
    string Street,
    string City,
    string Phone,
    string? Notes);

public sealed record OrderItemDto(
    Guid ProductId,
    string ProductNameEn,
    string ProductNameAr,
    string SKU,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal LineSubtotal,
    decimal LineDiscount,
    decimal LineTotal);
