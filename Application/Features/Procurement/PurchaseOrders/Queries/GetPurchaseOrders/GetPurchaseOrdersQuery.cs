using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Procurement.PurchaseOrders.Dtos;
using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Queries.GetPurchaseOrders;

public sealed record GetPurchaseOrdersQuery(PaginationRequest Pagination) : IRequest<PagedResult<PurchaseOrderDto>>;
