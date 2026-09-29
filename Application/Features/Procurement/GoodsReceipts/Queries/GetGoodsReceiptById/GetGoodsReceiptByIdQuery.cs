using Application.Features.Procurement.GoodsReceipts.Dtos;
using MediatR;

namespace Application.Features.Procurement.GoodsReceipts.Queries.GetGoodsReceiptById;

public sealed record GetGoodsReceiptByIdQuery(Guid Id) : IRequest<GoodsReceiptDto>;
