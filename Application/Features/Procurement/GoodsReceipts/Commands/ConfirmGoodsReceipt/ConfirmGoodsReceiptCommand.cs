using MediatR;

namespace Application.Features.Procurement.GoodsReceipts.Commands.ConfirmGoodsReceipt;

public sealed record ConfirmGoodsReceiptCommand(Guid GoodsReceiptId) : IRequest;
