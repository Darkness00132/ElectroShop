using Domain.Entities.OrdersAggregate;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Test.OrdersAggregate;

public class OrderTests
{
    [Fact]
    public void Order_Created_With_Pending_Status()
    {
        // Arrange & Act
        var order = CreateValidOrder();

        // Assert
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().BeEmpty();
        order.Total.Should().Be(0);
    }

    [Theory]
    [InlineData(1, 100, 0, 100)]
    [InlineData(2, 100, 10, 180)]
    [InlineData(3, 50, 5, 135)]
    public void Order_Total_Calculated_Correctly_When_Add_Item(
        int quantity,
        decimal unitPrice,
        decimal discountAmount,
        decimal expectedTotal)
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        order.AddItem(
            Guid.NewGuid(),
            "Gaming Laptop",
            "لابتوب ألعاب",
            "SKU-001",
            quantity,
            unitPrice,
            discountAmount);

        // Assert
        order.Subtotal.Should().Be(unitPrice * quantity);
        order.ItemsDiscountAmount.Should().Be(discountAmount * quantity);
        order.Total.Should().Be(expectedTotal);
    }

    [Fact]
    public void Order_Increases_Quantity_When_Add_Existing_Product()
    {
        // Arrange
        var order = CreateValidOrder();
        var productId = Guid.NewGuid();

        order.AddItem(productId, "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 2, 100);

        // Act
        order.AddItem(productId, "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 3, 100);

        // Assert
        order.Items.Should().ContainSingle();
        order.Items.Single().Quantity.Should().Be(5);
        order.Subtotal.Should().Be(500);
    }

    [Theory]
    [InlineData(20, 220)]
    [InlineData(50, 250)]
    [InlineData(0, 200)]
    public void Order_Total_Includes_Shipping_Fee(
        decimal shippingFee,
        decimal expectedTotal)
    {
        // Arrange
        var order = CreateValidOrder();
        order.AddItem(Guid.NewGuid(), "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 2, 100);

        // Act
        order.SetShippingFee(shippingFee);

        // Assert
        order.ShippingFee.Should().Be(shippingFee);
        order.Total.Should().Be(expectedTotal);
    }

    [Theory]
    [InlineData(10, 190)]
    [InlineData(30, 170)]
    [InlineData(200, 0)]
    public void Order_Total_Accounts_For_Promo_Discount(
        decimal promoDiscountAmount,
        decimal expectedTotal)
    {
        // Arrange
        var order = CreateValidOrder();
        var promoCodeId = Guid.NewGuid();

        order.AddItem(Guid.NewGuid(), "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 2, 100);

        // Act
        order.ApplyPromoCode(
            promoCodeId,
            promoDiscountAmount);

        // Assert
        order.PromoCodeId.Should().Be(promoCodeId);
        order.PromoDiscountAmount.Should().Be(promoDiscountAmount);
        order.Total.Should().Be(expectedTotal);
    }

    [Fact]
    public void Order_Returns_To_Original_Total_When_Remove_Promo_Code()
    {
        // Arrange
        var order = CreateValidOrder();
        order.AddItem(Guid.NewGuid(), "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 2, 100);
        order.ApplyPromoCode(Guid.NewGuid(), 30);

        // Act
        order.RemovePromoCode();

        // Assert
        order.PromoCodeId.Should().BeNull();
        order.PromoDiscountAmount.Should().Be(0);
        order.Total.Should().Be(200);
    }

    [Fact]
    public void Order_Changes_Shipping_Address_When_Order_Is_Pending()
    {
        // Arrange
        var order = CreateValidOrder();
        var newAddress = new Address(
            "new street",
            "new city",
            "01111111111",
            "new notes");

        // Act
        order.ChangeShippingAddress(newAddress);

        // Assert
        order.ShippingAddress.Should().Be(newAddress);
    }

    [Fact]
    public void Order_Cannot_Be_Confirmed_Without_Items()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Order_Follows_Confirmation_Workflow()
    {
        // Arrange
        var order = CreateValidOrder();
        order.AddItem(Guid.NewGuid(), "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 1, 100);

        // Act
        order.Confirm();
        order.StartProcessing();
        order.Ship();
        order.Deliver();

        // Assert
        order.Status.Should().Be(OrderStatus.Delivered);
        order.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Order_Can_Be_Refunded_After_Delivery()
    {
        // Arrange
        var order = CreateValidOrder();
        order.AddItem(Guid.NewGuid(), "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 1, 100);
        order.Confirm();
        order.StartProcessing();
        order.Ship();
        order.Deliver();

        // Act
        order.MarkAsRefunded();

        // Assert
        order.Status.Should().Be(OrderStatus.Refunded);
        order.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Order_Can_Be_Cancelled_Before_Completion()
    {
        // Arrange
        var order = CreateValidOrder();
        order.AddItem(Guid.NewGuid(), "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 1, 100);

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.IsCompleted.Should().BeFalse();
    }

    [Theory]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Processing)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Refunded)]
    public void Order_Cannot_Modify_Items_After_Pending_Status(OrderStatus status)
    {
        // Arrange
        var order = CreateOrderInStatus(status);

        // Act
        var act = () => order.AddItem(Guid.NewGuid(), "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 1, 100);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Processing)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Refunded)]
    public void Order_Cannot_Change_Shipping_Fee_After_Pending_Status(
        OrderStatus status)
    {
        // Arrange
        var order = CreateOrderInStatus(status);

        // Act
        var act = () => order.SetShippingFee(20);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Refunded)]
    public void Order_Is_Completed_When_Delivered_Or_Refunded(OrderStatus status)
    {
        // Arrange
        var order = CreateOrderInStatus(status);

        // Act & Assert
        order.IsCompleted.Should().BeTrue();
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Processing)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Cancelled)]
    public void Order_Is_Not_Completed_When_Not_Delivered_Or_Refunded(OrderStatus status)
    {
        // Arrange
        var order = CreateOrderInStatus(status);

        // Act & Assert
        order.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void Order_Cannot_Be_Cancelled_After_Delivery()
    {
        // Arrange
        var order = CreateOrderInStatus(OrderStatus.Delivered);

        // Act
        var act = () => order.Cancel();

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Order_Cannot_Be_Refunded_Before_Delivery()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        var act = () => order.MarkAsRefunded();

        // Assert
        act.Should().Throw<DomainException>();
    }

    private Order CreateValidOrder()
    {
        return new Order(
            Guid.NewGuid(),
            new Address(
                "street",
                "city",
                "01000000000",
                "notes"
            )
        );
    }

    private Order CreateOrderInStatus(OrderStatus status)
    {
        var order = CreateValidOrder();

        switch (status) {
            case OrderStatus.Pending:
                break;

            case OrderStatus.Confirmed:
                order.AddItem(Guid.NewGuid(), "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 1, 100);
                order.Confirm();
                break;

            case OrderStatus.Processing:
                order.AddItem(Guid.NewGuid(), "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 1, 100);
                order.Confirm();
                order.StartProcessing();
                break;

            case OrderStatus.Shipped:
                order.AddItem(Guid.NewGuid(), "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 1, 100);
                order.Confirm();
                order.StartProcessing();
                order.Ship();
                break;

            case OrderStatus.Delivered:
                order.AddItem(Guid.NewGuid(), "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 1, 100);
                order.Confirm();
                order.StartProcessing();
                order.Ship();
                order.Deliver();
                break;

            case OrderStatus.Cancelled:
                order.Cancel();
                break;

            case OrderStatus.Refunded:
                order.AddItem(Guid.NewGuid(), "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 1, 100);
                order.Confirm();
                order.StartProcessing();
                order.Ship();
                order.Deliver();
                order.MarkAsRefunded();
                break;
        }

        return order;
    }
}
