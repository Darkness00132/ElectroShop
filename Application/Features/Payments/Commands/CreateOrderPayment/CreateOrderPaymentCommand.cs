using MediatR;

namespace Application.Features.Payments.Commands.CreateOrderPayment;

public sealed record CreateOrderPaymentCommand(Guid OrderId) : IRequest<Guid>;
