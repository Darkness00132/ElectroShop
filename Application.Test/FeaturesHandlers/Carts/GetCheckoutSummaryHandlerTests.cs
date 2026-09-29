using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Application.Features.Carts.Queries.GetCheckoutSummary;
using Application.Settings;
using Domain.Entities.Carts;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.Test.FeaturesHandlers.Carts;

public class GetCheckoutSummaryHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    private readonly Mock<IRepository<Cart>> _cartRepositoryMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IRepository<Inventory>> _inventoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly GetCheckoutSummaryHandler _sut;

    public GetCheckoutSummaryHandlerTests()
    {
        _cartRepositoryMock = new Mock<IRepository<Cart>>();
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _inventoryRepositoryMock = new Mock<IRepository<Inventory>>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(_userId);

        _sut = new GetCheckoutSummaryHandler(
            _cartRepositoryMock.Object,
            _productRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _currentUserMock.Object,
            Options.Create(new ShippingSettings { Fee = 50m }));
    }

    [Fact]
    public async Task The_Checkout_Summary_Shows_Prices_Discounts_Stock_And_Totals()
    {
        // Arrange
        var product = CreateActiveProduct();
        var cart = new Cart(_userId);
        cart.AddItem(product.Id, 2);
        var inventory = new Inventory(product.Id, 5, reorderLevel: 0);

        SetupCartFound(cart);
        SetupProductsFound(product);
        SetupInventoriesFound(inventory);

        // Act
        var summary = await _sut.Handle(new GetCheckoutSummaryQuery(), CancellationToken.None);

        // Assert
        summary.Items.Should().ContainSingle();
        summary.Items.Single().LineSubtotal.Should().Be(400m);
        summary.Items.Single().LineDiscount.Should().Be(40m);
        summary.Items.Single().AvailableStock.Should().Be(5);

        summary.Subtotal.Should().Be(400m);
        summary.ItemsDiscountAmount.Should().Be(40m);
        summary.ShippingFee.Should().Be(50m);
        summary.Total.Should().Be(410m);
    }

    [Fact]
    public async Task The_Checkout_Summary_Cannot_Be_Retrieved_When_The_Cart_Is_Empty()
    {
        // Arrange
        SetupCartFound(new Cart(_userId));

        // Act
        var act = () => _sut.Handle(new GetCheckoutSummaryQuery(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
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

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        product.AssignDiscount(new Discount(
            "Summer Sale",
            Domain.Enums.DiscountType.Percentage,
            10,
            new Domain.ValueObjects.DateRange(today.AddDays(-1), today.AddDays(1))));

        return product;
    }

    private void SetupCartFound(Cart cart)
    {
        _cartRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, object?>>[]>()))
            .ReturnsAsync(cart);
    }

    private void SetupProductsFound(params Product[] products)
    {
        _productRepositoryMock
            .Setup(x => x.ListAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, object?>>[]>()))
            .ReturnsAsync(products.ToList());
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
