using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.OrdersAggregate;
using Domain.Entities.PaymentsAggregate;
using Domain.Enums;
using MediatR;

namespace Application.Features.Payments.Commands.CreateOrderPayment;

internal class CreateOrderPaymentHandler : IRequestHandler<CreateOrderPaymentCommand, Guid>
{
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderPaymentHandler(IRepository<Payment> paymentRepository, IRepository<Order> orderRepository, IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateOrderPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (await _paymentRepository.ExistsAsync(p => p.OrderId == order.Id, cancellationToken))
            throw new ConflictException("A payment already exists for this order.");

        var payment = new Payment(order.Id, order.Total);

        payment.AddAttempt(PaymentMethod.CashOnDelivery, order.Total);

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
