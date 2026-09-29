using Application.Abstractions.Repositories;
using Application.Common.Pagination;
using Application.Features.Payments.Dtos;
using Domain.Entities.PaymentsAggregate;
using MediatR;

namespace Application.Features.Payments.Queries.GetPayments;

internal class GetPaymentsHandler : IRequestHandler<GetPaymentsQuery, PagedResult<PaymentDto>>
{
    private readonly IRepository<Payment> _paymentRepository;

    public GetPaymentsHandler(IRepository<Payment> paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PagedResult<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        return await _paymentRepository.ProjectToPagedAsync<PaymentDto>(
            request.Pagination,
            orderBy: payment => payment.CreatedAt,
            descending: true,
            cancellationToken: cancellationToken);
    }
}
