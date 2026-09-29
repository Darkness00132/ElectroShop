using MediatR;

namespace Application.Features.Procurement.GoodsReceipts.Commands.AddGoodsReceiptItem;

public sealed record AddGoodsReceiptItemCommand(Guid GoodsReceiptId, Guid ProductId, int Quantity) : IRequest;
