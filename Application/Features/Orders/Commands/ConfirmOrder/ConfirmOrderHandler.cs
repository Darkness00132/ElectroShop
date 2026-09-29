using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.OrdersAggregate;
using MediatR;

namespace Application.Features.Orders.Commands.ConfirmOrder;

internal class ConfirmOrderHandler : IRequestHandler<ConfirmOrderCommand>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmOrderHandler(IRepository<Order> orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.Confirm();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
