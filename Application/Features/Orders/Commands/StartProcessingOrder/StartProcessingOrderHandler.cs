using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.OrdersAggregate;
using MediatR;

namespace Application.Features.Orders.Commands.StartProcessingOrder;

internal class StartProcessingOrderHandler : IRequestHandler<StartProcessingOrderCommand>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StartProcessingOrderHandler(IRepository<Order> orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(StartProcessingOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.StartProcessing();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
