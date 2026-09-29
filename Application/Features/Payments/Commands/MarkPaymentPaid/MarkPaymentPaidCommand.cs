using MediatR;

namespace Application.Features.Payments.Commands.MarkPaymentPaid;

public sealed record MarkPaymentPaidCommand(Guid PaymentId) : IRequest;
