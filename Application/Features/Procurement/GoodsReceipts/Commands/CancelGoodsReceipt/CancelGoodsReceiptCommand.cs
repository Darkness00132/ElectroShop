using MediatR;

namespace Application.Features.Procurement.GoodsReceipts.Commands.CancelGoodsReceipt;

public sealed record CancelGoodsReceiptCommand(Guid GoodsReceiptId) : IRequest;
