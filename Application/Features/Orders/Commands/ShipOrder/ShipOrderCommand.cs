using MediatR;

namespace Application.Features.Orders.Commands.ShipOrder;

public sealed record ShipOrderCommand(Guid OrderId) : IRequest;
