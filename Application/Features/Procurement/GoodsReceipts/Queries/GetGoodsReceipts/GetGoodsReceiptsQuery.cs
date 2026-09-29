using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Procurement.GoodsReceipts.Dtos;
using MediatR;

namespace Application.Features.Procurement.GoodsReceipts.Queries.GetGoodsReceipts;

public sealed record GetGoodsReceiptsQuery(PaginationRequest Pagination) : IRequest<PagedResult<GoodsReceiptDto>>;
