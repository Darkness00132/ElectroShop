using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.PaymentsAggregate;
using MediatR;

namespace Application.Features.Payments.Commands.MarkPaymentPaid;

internal class MarkPaymentPaidHandler : IRequestHandler<MarkPaymentPaidCommand>
{
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkPaymentPaidHandler(IRepository<Payment> paymentRepository, IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkPaymentPaidCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Payment), request.PaymentId);

        // MarkAsPaid is idempotent: marking an already paid payment again changes nothing.
        payment.MarkAsPaid();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
