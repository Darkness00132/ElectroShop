using MediatR;

namespace Application.Features.Orders.Commands.AdminCancelOrder;

public sealed record AdminCancelOrderCommand(Guid OrderId) : IRequest;
