using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Application.Features.Orders.Commands.CancelMyOrder;
using Domain.Entities.Carts;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.OrdersAggregate;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Orders;

public class CancelMyOrderHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    private readonly Mock<IRepository<Domain.Entities.OrdersAggregate.Order>> _orderRepositoryMock;
    private readonly Mock<IRepository<Inventory>> _inventoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly CancelMyOrderHandler _sut;

    public CancelMyOrderHandlerTests()
    {
        _orderRepositoryMock = new Mock<IRepository<Domain.Entities.OrdersAggregate.Order>>();
        _inventoryRepositoryMock = new Mock<IRepository<Inventory>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(_userId);

        _sut = new CancelMyOrderHandler(
            _orderRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task A_Pending_Order_Can_Be_Cancelled_And_Its_Stock_Is_Restored()
    {
        // Arrange
        var product = CreateActiveProduct();
        var order = CreateOrder(_userId, (product.Id, 2));
        var inventory = CreateInventory(product.Id, quantityOnHand: 8);

        SetupOrderFound(order);
        SetupInventoriesFound(inventory);

        // Act
        await _sut.Handle(new CancelMyOrderCommand(order.Id), CancellationToken.None);

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);

        inventory.QuantityOnHand.Should().Be(10);
        inventory.Transactions.Should().ContainSingle(t => t.OrderId == order.Id);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task An_Order_Cannot_Be_Cancelled_When_It_Belongs_To_Another_Customer()
    {
        // Arrange
        SetupOrderMissing();

        // Act
        var act = () => _sut.Handle(new CancelMyOrderCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task A_Shipped_Order_Cannot_Be_Cancelled_By_The_Customer()
    {
        // Arrange
        var product = CreateActiveProduct();
        var order = CreateOrder(_userId, (product.Id, 2));
        order.Confirm();
        order.StartProcessing();
        order.Ship();

        SetupOrderFound(order);

        // Act
        var act = () => _sut.Handle(new CancelMyOrderCommand(order.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        order.Status.Should().Be(OrderStatus.Shipped);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
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

    private static Domain.Entities.OrdersAggregate.Order CreateOrder(Guid userId, params (Guid ProductId, int Quantity)[] items)
    {
        var order = new Domain.Entities.OrdersAggregate.Order(
            userId,
            new Address("Main Street", "Cairo", "01000000000"));

        foreach (var (productId, quantity) in items)
            order.AddItem(productId, "Gaming Laptop", "لابتوب ألعاب", "SKU-001", quantity, 200m);

        return order;
    }

    private void SetupOrderFound(Domain.Entities.OrdersAggregate.Order order)
    {
        _orderRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.OrdersAggregate.Order, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.OrdersAggregate.Order, object?>>[]>()))
            .ReturnsAsync(order);
    }

    private void SetupOrderMissing()
    {
        _orderRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.OrdersAggregate.Order, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.OrdersAggregate.Order, object?>>[]>()))
            .ReturnsAsync((Domain.Entities.OrdersAggregate.Order?)null);
    }

    private void SetupInventoriesFound(params Inventory[] inventories)
    {
        _inventoryRepositoryMock
            .Setup(x => x.ListAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, object?>>[]>()))
            .ReturnsAsync(inventories.ToList());
    }
}
