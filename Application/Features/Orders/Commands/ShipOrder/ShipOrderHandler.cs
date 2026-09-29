using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.OrdersAggregate;
using MediatR;

namespace Application.Features.Orders.Commands.ShipOrder;

internal class ShipOrderHandler : IRequestHandler<ShipOrderCommand>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ShipOrderHandler(IRepository<Order> orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.Ship();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
