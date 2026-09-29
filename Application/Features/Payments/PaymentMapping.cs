using Application.Features.Payments.Dtos;
using AutoMapper;
using Domain.Entities.PaymentsAggregate;

namespace Application.Features.Payments;

internal class PaymentMapping : Profile
{
    public PaymentMapping()
    {
        CreateMap<Payment, PaymentDto>();
        CreateMap<PaymentAttempt, PaymentAttemptDto>();
    }
}
