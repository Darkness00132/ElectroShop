using MediatR;

namespace Application.Features.Orders.Commands.CancelMyOrder;

public sealed record CancelMyOrderCommand(Guid OrderId) : IRequest;
