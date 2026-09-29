using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Orders.Commands.AdminCancelOrder;
using Application.Features.Orders.Commands.ConfirmOrder;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.OrdersAggregate;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Orders;

public class OrderTransitionHandlerTests
{
    private readonly Mock<IRepository<Domain.Entities.OrdersAggregate.Order>> _orderRepositoryMock;
    private readonly Mock<IRepository<Inventory>> _inventoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public OrderTransitionHandlerTests()
    {
        _orderRepositoryMock = new Mock<IRepository<Domain.Entities.OrdersAggregate.Order>>();
        _inventoryRepositoryMock = new Mock<IRepository<Inventory>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task A_Pending_Order_Can_Be_Confirmed()
    {
        // Arrange
        var order = CreateOrder((Guid.NewGuid(), 1));
        SetupOrderFound(order);

        var handler = new ConfirmOrderHandler(_orderRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task An_Order_Cannot_Be_Confirmed_When_It_Does_Not_Exist()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.OrdersAggregate.Order?)null);

        var handler = new ConfirmOrderHandler(_orderRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new ConfirmOrderCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task An_Admin_Can_Cancel_An_Order_And_Its_Stock_Is_Restored()
    {
        // Arrange
        var product = CreateActiveProduct();
        var order = CreateOrder((product.Id, 2));
        var inventory = CreateInventory(product.Id, quantityOnHand: 8);

        _orderRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.OrdersAggregate.Order, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.OrdersAggregate.Order, object?>>[]>()))
            .ReturnsAsync(order);

        _inventoryRepositoryMock
            .Setup(x => x.ListAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, object?>>[]>()))
            .ReturnsAsync(new[] { inventory }.ToList());

        var handler = new AdminCancelOrderHandler(_orderRepositoryMock.Object, _inventoryRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new AdminCancelOrderCommand(order.Id), CancellationToken.None);

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        inventory.QuantityOnHand.Should().Be(10);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Product CreateActiveProduct()
    {
        var product = new Product(
            "Gaming Laptop",
            "لابتوب ألعاب",
            "A laptop for gaming.",
            "لابتوب للألعاب.",
            "SKU-001",
            200m,
            Guid.NewGuid(),
            Guid.NewGuid());

        product.Activate();

        return product;
    }

    private static Inventory CreateInventory(Guid productId, int quantityOnHand)
    {
        return new Inventory(productId, quantityOnHand, reorderLevel: 0);
    }

    private static Domain.Entities.OrdersAggregate.Order CreateOrder(params (Guid ProductId, int Quantity)[] items)
    {
        var order = new Domain.Entities.OrdersAggregate.Order(
            Guid.NewGuid(),
            new Address("Main Street", "Cairo", "01000000000"));

        foreach (var (productId, quantity) in items)
            order.AddItem(productId, "Gaming Laptop", "لابتوب ألعاب", "SKU-001", quantity, 200m);

        return order;
    }

    private void SetupOrderFound(Domain.Entities.OrdersAggregate.Order order)
    {
        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
    }
}
