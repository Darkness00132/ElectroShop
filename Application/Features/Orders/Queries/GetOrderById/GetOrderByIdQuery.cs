using Application.Features.Orders.Dtos;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto>;
