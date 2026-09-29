using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Application.Features.Carts.Commands.UpdateCartItem;
using Domain.Entities.Carts;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Carts;

public class UpdateCartItemHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    private readonly Mock<IRepository<Cart>> _cartRepositoryMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IRepository<Inventory>> _inventoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly UpdateCartItemHandler _sut;

    public UpdateCartItemHandlerTests()
    {
        _cartRepositoryMock = new Mock<IRepository<Cart>>();
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _inventoryRepositoryMock = new Mock<IRepository<Inventory>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(_userId);

        _sut = new UpdateCartItemHandler(
            _cartRepositoryMock.Object,
            _productRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task A_Cart_Item_Quantity_Can_Be_Changed()
    {
        // Arrange
        var product = CreateActiveProduct();
        var cart = CreateCartWithItem(product.Id, quantity: 1);
        var inventory = CreateInventory(product.Id, quantityOnHand: 10);

        SetupCartFound(cart);
        SetupProductFound(product);
        SetupInventoryFound(inventory);

        // Act
        await _sut.Handle(new UpdateCartItemCommand(product.Id, 5), CancellationToken.None);

        // Assert
        cart.Items.Should().ContainSingle(item => item.Quantity == 5);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Cart_Item_Cannot_Be_Updated_When_It_Does_Not_Exist()
    {
        // Arrange
        var cart = CreateCartWithItem(Guid.NewGuid(), quantity: 1);

        SetupCartFound(cart);

        // Act
        var act = () => _sut.Handle(new UpdateCartItemCommand(Guid.NewGuid(), 5), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task A_Cart_Item_Cannot_Be_Updated_When_There_Is_Not_Enough_Stock()
    {
        // Arrange
        var product = CreateActiveProduct();
        var cart = CreateCartWithItem(product.Id, quantity: 1);
        var inventory = CreateInventory(product.Id, quantityOnHand: 4);

        SetupCartFound(cart);
        SetupProductFound(product);
        SetupInventoryFound(inventory);

        // Act
        var act = () => _sut.Handle(new UpdateCartItemCommand(product.Id, 5), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        cart.Items.Should().ContainSingle(item => item.Quantity == 1);

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
            1000m,
            Guid.NewGuid(),
            Guid.NewGuid());

        product.Activate();

        return product;
    }

    private static Inventory CreateInventory(Guid productId, int quantityOnHand)
    {
        return new Inventory(productId, quantityOnHand, reorderLevel: 0);
    }

    private Cart CreateCartWithItem(Guid productId, int quantity)
    {
        var cart = new Cart(_userId);
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

    private void SetupProductFound(Product product)
    {
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
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
