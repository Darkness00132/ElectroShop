using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.OrdersAggregate;
using Domain.Enums;
using MediatR;

namespace Application.Features.Orders.Commands.CancelMyOrder;

internal class CancelMyOrderHandler : IRequestHandler<CancelMyOrderCommand>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CancelMyOrderHandler(IRepository<Order> orderRepository, IRepository<Inventory> inventoryRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(CancelMyOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.SingleOrDefaultAsync(
            o => o.Id == request.OrderId && o.UserId == _currentUser.UserId,
            cancellationToken,
            o => o.Items)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (order.Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
            throw new ConflictException("The order can no longer be cancelled.");

        order.Cancel();

        await RestoreStockAsync(order, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task RestoreStockAsync(Order order, CancellationToken cancellationToken)
    {
        var productIds = order.Items.Select(item => item.ProductId).ToList();

        var inventories = await _inventoryRepository.ListAsync(
            i => productIds.Contains(i.ProductId),
            cancellationToken);

        foreach (var item in order.Items) {
            inventories
                .Single(inventory => inventory.ProductId == item.ProductId)
                .IncreaseStock(item.Quantity, order.Id);
        }
    }
}
