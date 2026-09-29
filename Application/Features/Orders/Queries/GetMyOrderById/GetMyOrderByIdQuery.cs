using Application.Features.Orders.Dtos;
using MediatR;

namespace Application.Features.Orders.Queries.GetMyOrderById;

public sealed record GetMyOrderByIdQuery(Guid Id) : IRequest<OrderDto>;
