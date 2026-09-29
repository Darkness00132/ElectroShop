using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Application.Features.Carts.Commands.AddCartItem;
using Domain.Entities.Carts;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Carts;

public class AddCartItemHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    private readonly Mock<IRepository<Cart>> _cartRepositoryMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IRepository<Inventory>> _inventoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly AddCartItemHandler _sut;

    public AddCartItemHandlerTests()
    {
        _cartRepositoryMock = new Mock<IRepository<Cart>>();
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _inventoryRepositoryMock = new Mock<IRepository<Inventory>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(_userId);

        _sut = new AddCartItemHandler(
            _cartRepositoryMock.Object,
            _productRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task An_Item_Can_Be_Added_To_The_Cart()
    {
        // Arrange
        var product = CreateActiveProduct();
        var cart = CreateCart();
        var inventory = CreateInventory(product.Id, quantityOnHand: 10);

        SetupProductFound(product);
        SetupInventoryFound(inventory);
        SetupCartFound(cart);

        // Act
        await _sut.Handle(new AddCartItemCommand(product.Id, 2), CancellationToken.None);

        // Assert
        cart.Items.Should().ContainSingle(item => item.ProductId == product.Id && item.Quantity == 2);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Adding_A_Product_That_Is_Already_In_The_Cart_Increases_Its_Quantity()
    {
        // Arrange
        var product = CreateActiveProduct();
        var cart = CreateCart();
        cart.AddItem(product.Id, 1);

        var inventory = CreateInventory(product.Id, quantityOnHand: 10);

        SetupProductFound(product);
        SetupInventoryFound(inventory);
        SetupCartFound(cart);

        // Act
        await _sut.Handle(new AddCartItemCommand(product.Id, 2), CancellationToken.None);

        // Assert
        cart.Items.Should().ContainSingle(item => item.Quantity == 3);
    }

    [Fact]
    public async Task A_Cart_Is_Created_When_The_Customer_Does_Not_Have_One_Yet()
    {
        // Arrange
        var product = CreateActiveProduct();
        var inventory = CreateInventory(product.Id, quantityOnHand: 10);

        Cart? addedCart = null;

        SetupProductFound(product);
        SetupInventoryFound(inventory);
        SetupCartMissing();

        _cartRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
            .Callback<Cart, CancellationToken>((cart, _) => addedCart = cart);

        // Act
        await _sut.Handle(new AddCartItemCommand(product.Id, 1), CancellationToken.None);

        // Assert
        addedCart.Should().NotBeNull();
        addedCart!.UserId.Should().Be(_userId);
        addedCart.Items.Should().ContainSingle(item => item.ProductId == product.Id);
    }

    [Fact]
    public async Task An_Item_Cannot_Be_Added_When_The_Product_Does_Not_Exist()
    {
        // Arrange
        var command = new AddCartItemCommand(Guid.NewGuid(), 1);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task An_Item_Cannot_Be_Added_When_The_Product_Is_Not_Active()
    {
        // Arrange
        var product = CreateActiveProduct();
        product.Deactivate();

        SetupProductFound(product);

        // Act
        var act = () => _sut.Handle(new AddCartItemCommand(product.Id, 1), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task An_Item_Cannot_Be_Added_When_The_Total_Quantity_Exceeds_The_Available_Stock()
    {
        // Arrange
        var product = CreateActiveProduct();
        var cart = CreateCart();
        cart.AddItem(product.Id, 8);

        var inventory = CreateInventory(product.Id, quantityOnHand: 10);

        SetupProductFound(product);
        SetupInventoryFound(inventory);
        SetupCartFound(cart);

        // Act
        var act = () => _sut.Handle(new AddCartItemCommand(product.Id, 3), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        cart.Items.Should().ContainSingle(item => item.Quantity == 8);

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

    private Cart CreateCart()
    {
        return new Cart(_userId);
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
}
