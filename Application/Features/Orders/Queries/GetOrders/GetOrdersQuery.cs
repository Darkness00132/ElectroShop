using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Orders.Dtos;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery(PaginationRequest Pagination) : IRequest<PagedResult<OrderSummaryDto>>;
