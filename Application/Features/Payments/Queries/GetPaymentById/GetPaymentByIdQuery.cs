using Application.Features.Payments.Dtos;
using MediatR;

namespace Application.Features.Payments.Queries.GetPaymentById;

public sealed record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentDto>;
