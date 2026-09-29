using MediatR;

namespace Application.Features.Orders.Commands.DeliverOrder;

public sealed record DeliverOrderCommand(Guid OrderId) : IRequest;
