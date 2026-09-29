using MediatR;

namespace Application.Features.Procurement.GoodsReceipts.Commands.CreateGoodsReceipt;

public sealed record CreateGoodsReceiptCommand(
    string Number,
    Guid PurchaseOrderId,
    DateTime ReceivedAt,
    string? DeliveryReference,
    string? Notes) : IRequest<Guid>;
