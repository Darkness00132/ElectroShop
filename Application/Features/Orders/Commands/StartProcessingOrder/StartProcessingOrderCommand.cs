using MediatR;

namespace Application.Features.Orders.Commands.StartProcessingOrder;

public sealed record StartProcessingOrderCommand(Guid OrderId) : IRequest;
