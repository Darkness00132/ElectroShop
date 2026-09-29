using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.OrdersAggregate;
using MediatR;

namespace Application.Features.Orders.Commands.AdminCancelOrder;

internal class AdminCancelOrderHandler : IRequestHandler<AdminCancelOrderCommand>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminCancelOrderHandler(IRepository<Order> orderRepository, IRepository<Inventory> inventoryRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AdminCancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.SingleOrDefaultAsync(
            o => o.Id == request.OrderId,
            cancellationToken,
            o => o.Items)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.Cancel();

        var productIds = order.Items.Select(item => item.ProductId).ToList();

        var inventories = await _inventoryRepository.ListAsync(
            i => productIds.Contains(i.ProductId),
            cancellationToken);

        foreach (var item in order.Items) {
            inventories
                .Single(inventory => inventory.ProductId == item.ProductId)
                .IncreaseStock(item.Quantity, order.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
