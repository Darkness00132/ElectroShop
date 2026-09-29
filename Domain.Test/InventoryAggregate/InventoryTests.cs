using Domain.Entities.InventoryAggregate;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;

namespace Domain.Test.InventoryAggregate;

public class InventoryTests
{
    [Fact]
    public void Inventory_Created_When_Provide_Valid_Data()
    {
        // Arrange & Act
        var inventory = CreateValidInventory();

        // Assert
        inventory.ProductId.Should().NotBe(Guid.Empty);
        inventory.QuantityOnHand.Should().Be(10);
        inventory.ReorderLevel.Should().Be(5);
        inventory.Transactions.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(-10, 5)]
    [InlineData(10, -1)]
    [InlineData(10, -10)]
    public void Inventory_Creation_Fails_When_Provide_Negative_Quantities(
        int quantityOnHand,
        int reorderLevel)
    {
        // Arrange & Act
        var act = () => new Inventory(
            Guid.NewGuid(),
            quantityOnHand,
            reorderLevel);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Inventory_Increases_Stock_And_Creates_Transaction_When_Provide_Valid_Quantity()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        inventory.IncreaseStock(10);

        // Assert
        inventory.Transactions.Should().ContainSingle();

        var transaction = inventory.Transactions.Single();

        transaction.Type.Should().Be(InventoryTransactionType.StockIn);
        transaction.QuantityChange.Should().Be(10);
        transaction.QuantityBefore.Should().Be(10);
        transaction.QuantityAfter.Should().Be(20);
        transaction.InventoryId.Should().Be(inventory.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Inventory_Increase_Stock_Fails_When_Provide_Invalid_Quantity(int quantity)
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        var act = () => inventory.IncreaseStock(quantity);

        // Assert
        act.Should().Throw<DomainException>();
        inventory.QuantityOnHand.Should().Be(10);
        inventory.Transactions.Should().BeEmpty();
    }


    [Fact]
    public void Inventory_Decreases_Stock_And_Creates_Transaction_When_Provide_Valid_Quantity()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        inventory.DecreaseStock(5);

        // Assert
        inventory.Transactions.Should().ContainSingle();

        var transaction = inventory.Transactions.Single();

        transaction.Type.Should().Be(InventoryTransactionType.StockOut);
        transaction.QuantityChange.Should().Be(-5);
        transaction.QuantityBefore.Should().Be(10);
        transaction.QuantityAfter.Should().Be(5);
        transaction.InventoryId.Should().Be(inventory.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Inventory_Decrease_Stock_Fails_When_Provide_Invalid_Quantity(int quantity)
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        var act = () => inventory.DecreaseStock(quantity);

        // Assert
        act.Should().Throw<DomainException>();
        inventory.QuantityOnHand.Should().Be(10);
        inventory.Transactions.Should().BeEmpty();
    }

    [Fact]
    public void Inventory_Decrease_Stock_Fails_When_Quantity_Exceeds_Stock()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        var act = () => inventory.DecreaseStock(11);

        // Assert
        act.Should().Throw<DomainException>();
        inventory.QuantityOnHand.Should().Be(10);
        inventory.Transactions.Should().BeEmpty();
    }

    [Fact]
    public void Inventory_Adjusts_Stock_And_Creates_Transaction_When_Provide_Valid_Quantity()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        inventory.AdjustStock(20);

        // Assert
        inventory.Transactions.Should().ContainSingle();

        var transaction = inventory.Transactions.Single();

        transaction.Type.Should().Be(InventoryTransactionType.Adjustment);
        transaction.QuantityChange.Should().Be(10);
        transaction.QuantityBefore.Should().Be(10);
        transaction.QuantityAfter.Should().Be(20);
        transaction.InventoryId.Should().Be(inventory.Id);
    }

    [Fact]
    public void Inventory_Adjusts_Stock_To_Zero_When_Provide_Zero_Quantity()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        inventory.AdjustStock(0);

        // Assert
        inventory.QuantityOnHand.Should().Be(0);
        inventory.Transactions.Should().ContainSingle();

        var transaction = inventory.Transactions.Single();

        transaction.Type.Should().Be(InventoryTransactionType.Adjustment);
        transaction.QuantityChange.Should().Be(-10);
        transaction.QuantityBefore.Should().Be(10);
        transaction.QuantityAfter.Should().Be(0);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Inventory_Adjust_Stock_Fails_When_Provide_Negative_Quantity(int quantity)
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        var act = () => inventory.AdjustStock(quantity);

        // Assert
        act.Should().Throw<DomainException>();
        inventory.QuantityOnHand.Should().Be(10);
        inventory.Transactions.Should().BeEmpty();
    }

    [Fact]
    public void Inventory_Changes_Reorder_Level_When_Provide_Valid_Level()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        inventory.ChangeReorderLevel(20);

        // Assert
        inventory.ReorderLevel.Should().Be(20);
    }

    [Fact]
    public void Inventory_Changes_Reorder_Level_To_Zero_When_Provide_Zero()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        inventory.ChangeReorderLevel(0);

        // Assert
        inventory.ReorderLevel.Should().Be(0);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Inventory_Change_Reorder_Level_Fails_When_Provide_Negative_Level(
        int reorderLevel)
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        var act = () => inventory.ChangeReorderLevel(reorderLevel);

        // Assert
        act.Should().Throw<DomainException>();
        inventory.ReorderLevel.Should().Be(5);
    }

    [Fact]
    public void Inventory_Increases_Stock_With_Goods_Receipt_When_Provide_Goods_Receipt_Id()
    {
        // Arrange
        var inventory = CreateValidInventory();
        var goodsReceiptId = Guid.NewGuid();

        // Act
        inventory.IncreaseStock(10, goodsReceiptId: goodsReceiptId);

        // Assert
        var transaction = inventory.Transactions.Single();

        transaction.GoodsReceiptId.Should().Be(goodsReceiptId);
        transaction.OrderId.Should().BeNull();
    }

    [Fact]
    public void Inventory_Decreases_Stock_With_Order_When_Provide_Order_Id()
    {
        // Arrange
        var inventory = CreateValidInventory();
        var orderId = Guid.NewGuid();

        // Act
        inventory.DecreaseStock(5, orderId);

        // Assert
        var transaction = inventory.Transactions.Single();

        transaction.OrderId.Should().Be(orderId);
        transaction.GoodsReceiptId.Should().BeNull();
    }

    [Fact]
    public void Inventory_Increases_Stock_With_Notes_When_Provide_Notes()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        inventory.IncreaseStock(10, notes: "Stock received");

        // Assert
        var transaction = inventory.Transactions.Single();

        transaction.Notes.Should().Be("Stock received");
    }

    private Inventory CreateValidInventory()
    {
        return new Inventory(Guid.NewGuid(), 10, 5);
    }
}
