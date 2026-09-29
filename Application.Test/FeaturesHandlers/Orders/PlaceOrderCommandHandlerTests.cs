using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Application.Features.Orders.Commands.PlaceOrder;
using Application.Settings;
using Domain.Entities.Carts;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.PromotionsAggregate;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.Test.FeaturesHandlers.Orders;

public class PlaceOrderCommandHandlerTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly Guid _userId = Guid.NewGuid();

    private readonly Mock<IRepository<Cart>> _cartRepositoryMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IRepository<Inventory>> _inventoryRepositoryMock;
    private readonly Mock<IRepository<PromoCode>> _promoCodeRepositoryMock;
    private readonly Mock<IRepository<Domain.Entities.OrdersAggregate.Order>> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly PlaceOrderHandler _sut;

    public PlaceOrderCommandHandlerTests()
    {
        _cartRepositoryMock = new Mock<IRepository<Cart>>();
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _inventoryRepositoryMock = new Mock<IRepository<Inventory>>();
        _promoCodeRepositoryMock = new Mock<IRepository<PromoCode>>();
        _orderRepositoryMock = new Mock<IRepository<Domain.Entities.OrdersAggregate.Order>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(_userId);

        _sut = new PlaceOrderHandler(
            _cartRepositoryMock.Object,
            _productRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _promoCodeRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserMock.Object,
            Options.Create(new ShippingSettings { Fee = 50m }));
    }

    [Fact]
    public async Task An_Order_Can_Be_Placed_From_The_Cart()
    {
        // Arrange
        var product = CreateActiveProduct(price: 200m);
        var cart = CreateCart((product.Id, 2));
        var inventory = CreateInventory(product.Id, quantityOnHand: 5);

        SetupCartFound(cart);
        SetupProductsFound(product);
        SetupInventoriesFound(inventory);

        Domain.Entities.OrdersAggregate.Order? addedOrder = null;

        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.OrdersAggregate.Order>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.OrdersAggregate.Order, CancellationToken>((order, _) => addedOrder = order);

        // Act
        var orderId = await _sut.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        orderId.Should().Be(addedOrder!.Id);

        addedOrder.UserId.Should().Be(_userId);
        addedOrder.Status.Should().Be(OrderStatus.Pending);
        addedOrder.Subtotal.Should().Be(400m);
        addedOrder.ShippingFee.Should().Be(50m);
        addedOrder.Total.Should().Be(450m);
        addedOrder.ShippingAddress.Street.Should().Be("Main Street");

        inventory.QuantityOnHand.Should().Be(3);
        inventory.Transactions.Should().ContainSingle(t => t.OrderId == orderId);

        cart.Items.Should().BeEmpty();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task The_Order_Lines_Carry_The_Product_Name_And_SKU_Snapshots()
    {
        // Arrange
        var product = CreateActiveProduct(price: 200m);
        var cart = CreateCart((product.Id, 1));
        var inventory = CreateInventory(product.Id, quantityOnHand: 5);

        SetupCartFound(cart);
        SetupProductsFound(product);
        SetupInventoriesFound(inventory);

        Domain.Entities.OrdersAggregate.Order? addedOrder = null;

        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.OrdersAggregate.Order>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.OrdersAggregate.Order, CancellationToken>((order, _) => addedOrder = order);

        // Act
        await _sut.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        addedOrder!.Items.Should().ContainSingle();
        addedOrder.Items.Single().ProductId.Should().Be(product.Id);
        addedOrder.Items.Single().ProductNameEn.Should().Be("Gaming Laptop");
        addedOrder.Items.Single().ProductNameAr.Should().Be("لابتوب ألعاب");
        addedOrder.Items.Single().SKU.Should().Be("SKU-001");
        addedOrder.Items.Single().UnitPrice.Should().Be(200m);
    }

    [Fact]
    public async Task A_Valid_Promo_Code_Is_Applied_And_Marked_As_Used()
    {
        // Arrange
        var product = CreateActiveProduct(price: 200m);
        var cart = CreateCart((product.Id, 2));
        var inventory = CreateInventory(product.Id, quantityOnHand: 5);
        var promo = CreatePromoCode("SAVE10", PromoDiscountType.Percentage, value: 10, minimumOrder: 100);

        SetupCartFound(cart);
        SetupProductsFound(product);
        SetupInventoriesFound(inventory);
        SetupPromoFound(promo);

        Domain.Entities.OrdersAggregate.Order? addedOrder = null;

        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.OrdersAggregate.Order>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.OrdersAggregate.Order, CancellationToken>((order, _) => addedOrder = order);

        // Act
        await _sut.Handle(CreateCommand(promoCode: "save10"), CancellationToken.None);

        // Assert
        addedOrder!.PromoCodeId.Should().Be(promo.Id);
        addedOrder.PromoDiscountAmount.Should().Be(40m);
        addedOrder.Total.Should().Be(410m);

        promo.UsedCount.Should().Be(1);
    }

    [Fact]
    public async Task An_Order_Cannot_Be_Placed_When_The_Cart_Is_Empty()
    {
        // Arrange
        SetupCartFound(CreateCart());

        // Act
        var act = () => _sut.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        _orderRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.OrdersAggregate.Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task An_Order_Cannot_Be_Placed_When_A_Product_Is_Not_Active()
    {
        // Arrange
        var product = CreateActiveProduct(price: 200m);
        product.Deactivate();

        SetupCartFound(CreateCart((product.Id, 1)));
        SetupProductsFound(product);
        SetupInventoriesFound(CreateInventory(product.Id, quantityOnHand: 5));

        // Act
        var act = () => _sut.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task An_Order_Cannot_Be_Placed_When_There_Is_Not_Enough_Stock()
    {
        // Arrange
        var product = CreateActiveProduct(price: 200m);

        SetupCartFound(CreateCart((product.Id, 2)));
        SetupProductsFound(product);
        SetupInventoriesFound(CreateInventory(product.Id, quantityOnHand: 1));

        // Act
        var act = () => _sut.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task An_Order_Cannot_Be_Placed_With_An_Invalid_Promo_Code()
    {
        // Arrange
        var product = CreateActiveProduct(price: 200m);

        SetupCartFound(CreateCart((product.Id, 1)));
        SetupProductsFound(product);
        SetupInventoriesFound(CreateInventory(product.Id, quantityOnHand: 5));
        SetupPromoMissing();

        // Act
        var act = () => _sut.Handle(CreateCommand(promoCode: "NOPE"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    private static PlaceOrderCommand CreateCommand(string? promoCode = null)
    {
        return new PlaceOrderCommand(
            Street: "Main Street",
            City: "Cairo",
            Phone: "01000000000",
            Notes: null,
            PromoCode: promoCode);
    }

    private static Product CreateActiveProduct(decimal price)
    {
        var product = new Product(
            "Gaming Laptop",
            "لابتوب ألعاب",
            "A laptop for gaming.",
            "لابتوب للألعاب.",
            "SKU-001",
            price,
            Guid.NewGuid(),
            Guid.NewGuid());

        product.Activate();

        return product;
    }

    private static Inventory CreateInventory(Guid productId, int quantityOnHand)
    {
        return new Inventory(productId, quantityOnHand, reorderLevel: 0);
    }

    private static PromoCode CreatePromoCode(string code, PromoDiscountType discountType, decimal value, decimal minimumOrder)
    {
        return new PromoCode(
            code,
            discountType,
            value,
            minimumOrder,
            new DateRange(Today.AddDays(-1), Today.AddDays(1)));
    }

    private Cart CreateCart(params (Guid ProductId, int Quantity)[] items)
    {
        var cart = new Cart(_userId);

        foreach (var (productId, quantity) in items)
            cart.AddItem(productId, quantity);

        return cart;
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

    private void SetupPromoFound(PromoCode promo)
    {
        _promoCodeRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<PromoCode, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(promo);
    }

    private void SetupPromoMissing()
    {
        _promoCodeRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<PromoCode, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PromoCode?)null);
    }
}
