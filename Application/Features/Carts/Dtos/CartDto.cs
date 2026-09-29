namespace Application.Features.Carts.Dtos;

public sealed record CartDto(
    Guid UserId,
    IReadOnlyList<CartItemDto> Items,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total);

public sealed record CartItemDto(
    Guid ProductId,
    string NameEn,
    string NameAr,
    decimal UnitPrice,
    int Quantity,
    decimal LineSubtotal,
    decimal LineDiscount,
    decimal LineTotal);
