using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.OrdersAggregate;
using Domain.Entities.PaymentsAggregate;
using MediatR;

namespace Application.Features.Payments.Commands.RefundPayment;

internal class RefundPaymentHandler : IRequestHandler<RefundPaymentCommand>
{
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RefundPaymentHandler(IRepository<Payment> paymentRepository, IRepository<Order> orderRepository, IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Payment), request.PaymentId);

        payment.Refund();

        var order = await _orderRepository.GetByIdAsync(payment.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), payment.OrderId);

        order.MarkAsRefunded();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
