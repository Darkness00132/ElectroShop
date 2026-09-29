using Application.Common.Pagination;
using Application.Features.Orders.Dtos;
using MediatR;

namespace Application.Features.Orders.Queries.GetMyOrders;

public sealed record GetMyOrdersQuery : IRequest<IReadOnlyList<OrderSummaryDto>>;
