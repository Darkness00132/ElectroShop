using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Payments.Dtos;
using Domain.Entities.PaymentsAggregate;
using MediatR;

namespace Application.Features.Payments.Queries.GetPaymentById;

internal class GetPaymentByIdHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto>
{
    private readonly IRepository<Payment> _paymentRepository;

    public GetPaymentByIdHandler(IRepository<Payment> paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.ProjectToSingleOrDefaultAsync<PaymentDto>(
            p => p.Id == request.Id,
            cancellationToken);

        if (payment is null)
            throw new NotFoundException(nameof(Payment), request.Id);

        return payment;
    }
}
