using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Inventories.Commands.AdjustStock;
using Application.Features.Inventories.Commands.ChangeReorderLevel;
using Application.Features.Inventories.Commands.StockIn;
using Application.Features.Inventories.Commands.StockOut;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Inventories;

public class InventoryHandlerTests
{
    private readonly Mock<IRepository<Inventory>> _inventoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public InventoryHandlerTests()
    {
        _inventoryRepositoryMock = new Mock<IRepository<Inventory>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task Stock_Can_Be_Received_And_Is_Recorded_As_A_Transaction()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var inventory = CreateInventory(productId, quantityOnHand: 10);
        SetupInventoryFound(inventory);

        var handler = new StockInHandler(_inventoryRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new StockInCommand(productId, 5, "Supplier delivery"), CancellationToken.None);

        // Assert
        inventory.QuantityOnHand.Should().Be(15);
        inventory.Transactions.Should().ContainSingle(t =>
            t.Type == InventoryTransactionType.StockIn &&
            t.QuantityChange == 5 &&
            t.Notes == "Supplier delivery");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Stock_Cannot_Be_Removed_When_It_Exceeds_The_Quantity_On_Hand()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var inventory = CreateInventory(productId, quantityOnHand: 10);
        SetupInventoryFound(inventory);

        var handler = new StockOutHandler(_inventoryRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new StockOutCommand(productId, 11, null), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();

        inventory.QuantityOnHand.Should().Be(10);
    }

    [Fact]
    public async Task Stock_Can_Be_Adjusted_To_An_Absolute_Quantity()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var inventory = CreateInventory(productId, quantityOnHand: 10);
        SetupInventoryFound(inventory);

        var handler = new AdjustStockHandler(_inventoryRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new AdjustStockCommand(productId, 7, "Stock count"), CancellationToken.None);

        // Assert
        inventory.QuantityOnHand.Should().Be(7);
        inventory.Transactions.Should().ContainSingle(t =>
            t.Type == InventoryTransactionType.Adjustment &&
            t.QuantityChange == -3);
    }

    [Fact]
    public async Task The_Reorder_Level_Can_Be_Changed()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var inventory = CreateInventory(productId, quantityOnHand: 10);
        SetupInventoryFound(inventory);

        var handler = new ChangeReorderLevelHandler(_inventoryRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new ChangeReorderLevelCommand(productId, 4), CancellationToken.None);

        // Assert
        inventory.ReorderLevel.Should().Be(4);
    }

    [Fact]
    public async Task Stock_Cannot_Be_Changed_When_The_Product_Has_No_Inventory()
    {
        // Arrange
        _inventoryRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Inventory?)null);

        var handler = new StockInHandler(_inventoryRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new StockInCommand(Guid.NewGuid(), 5, null), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static Inventory CreateInventory(Guid productId, int quantityOnHand)
    {
        return new Inventory(productId, quantityOnHand, reorderLevel: 2);
    }

    private void SetupInventoryFound(Inventory inventory)
    {
        _inventoryRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);
    }
}
