using Domain.Enums;

namespace Application.Features.Procurement.GoodsReceipts.Dtos;

public sealed record GoodsReceiptDto(
    Guid Id,
    string Number,
    Guid PurchaseOrderId,
    GoodsReceiptStatus Status,
    DateTime ReceivedAt,
    string? DeliveryReference,
    string? Notes,
    DateTime CreatedAt,
    IReadOnlyList<GoodsReceiptItemDto> Items);

public sealed record GoodsReceiptItemDto(
    Guid ProductId,
    string ProductNameEn,
    int Quantity);
