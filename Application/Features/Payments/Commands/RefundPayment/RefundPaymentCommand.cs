using MediatR;

namespace Application.Features.Payments.Commands.RefundPayment;

public sealed record RefundPaymentCommand(Guid PaymentId) : IRequest;
