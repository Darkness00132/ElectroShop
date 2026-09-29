using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Features.Carts.Queries.GetMyCart;
using Domain.Entities.Carts;
using Domain.Entities.Catalog;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Carts;

public class GetMyCartHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    private readonly Mock<IRepository<Cart>> _cartRepositoryMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly GetMyCartHandler _sut;

    public GetMyCartHandlerTests()
    {
        _cartRepositoryMock = new Mock<IRepository<Cart>>();
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(_userId);

        _sut = new GetMyCartHandler(
            _cartRepositoryMock.Object,
            _productRepositoryMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task An_Empty_Cart_Is_Returned_When_The_Customer_Has_No_Cart()
    {
        // Arrange
        SetupCartMissing();

        // Act
        var cart = await _sut.Handle(new GetMyCartQuery(), CancellationToken.None);

        // Assert
        cart.UserId.Should().Be(_userId);
        cart.Items.Should().BeEmpty();
        cart.Subtotal.Should().Be(0m);
        cart.DiscountAmount.Should().Be(0m);
        cart.Total.Should().Be(0m);
    }

    [Fact]
    public async Task The_Cart_Contains_Items_With_Prices_And_Active_Discounts()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var discountedProduct = CreateActiveProduct("SKU-001", price: 200m);
        discountedProduct.AssignDiscount(new Discount(
            "Summer Sale",
            DiscountType.Percentage,
            10,
            new DateRange(today.AddDays(-1), today.AddDays(1))));

        var fullPriceProduct = CreateActiveProduct("SKU-002", price: 100m);

        var cart = new Cart(_userId);
        cart.AddItem(discountedProduct.Id, 2);
        cart.AddItem(fullPriceProduct.Id, 1);

        SetupCartFound(cart);
        SetupProductsFound(discountedProduct, fullPriceProduct);

        // Act
        var cartDto = await _sut.Handle(new GetMyCartQuery(), CancellationToken.None);

        // Assert
        cartDto.Items.Should().HaveCount(2);

        var discountedLine = cartDto.Items.Single(item => item.ProductId == discountedProduct.Id);
        discountedLine.UnitPrice.Should().Be(200m);
        discountedLine.LineSubtotal.Should().Be(400m);
        discountedLine.LineDiscount.Should().Be(40m);
        discountedLine.LineTotal.Should().Be(360m);

        var fullPriceLine = cartDto.Items.Single(item => item.ProductId == fullPriceProduct.Id);
        fullPriceLine.LineDiscount.Should().Be(0m);
        fullPriceLine.LineTotal.Should().Be(100m);

        cartDto.Subtotal.Should().Be(500m);
        cartDto.DiscountAmount.Should().Be(40m);
        cartDto.Total.Should().Be(460m);
    }

    [Fact]
    public async Task Inactive_Discounts_Are_Not_Applied()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var product = CreateActiveProduct("SKU-001", price: 200m);
        var discount = new Discount(
            "Expired Sale",
            DiscountType.Percentage,
            10,
            new DateRange(today.AddDays(-10), today.AddDays(-5)));
        discount.Activate();
        product.AssignDiscount(discount);

        var cart = new Cart(_userId);
        cart.AddItem(product.Id, 1);

        SetupCartFound(cart);
        SetupProductsFound(product);

        // Act
        var cartDto = await _sut.Handle(new GetMyCartQuery(), CancellationToken.None);

        // Assert
        cartDto.Items.Should().ContainSingle();
        cartDto.Items.Single().LineDiscount.Should().Be(0m);
        cartDto.Total.Should().Be(200m);
    }

    private static Product CreateActiveProduct(string sku, decimal price)
    {
        var product = new Product(
            "Gaming Laptop",
            "لابتوب ألعاب",
            "A laptop for gaming.",
            "لابتوب للألعاب.",
            sku,
            price,
            Guid.NewGuid(),
            Guid.NewGuid());

        product.Activate();

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

    private void SetupCartMissing()
    {
        _cartRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, object?>>[]>()))
            .ReturnsAsync((Cart?)null);
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
}
