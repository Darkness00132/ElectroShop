using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Application.Features.Carts.Commands.RemoveCartItem;
using Domain.Entities.Carts;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Carts;

public class RemoveCartItemHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    private readonly Mock<IRepository<Cart>> _cartRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly RemoveCartItemHandler _sut;

    public RemoveCartItemHandlerTests()
    {
        _cartRepositoryMock = new Mock<IRepository<Cart>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(_userId);

        _sut = new RemoveCartItemHandler(
            _cartRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task A_Cart_Item_Can_Be_Removed()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var cart = new Cart(_userId);
        cart.AddItem(productId, 2);

        SetupCartFound(cart);

        // Act
        await _sut.Handle(new RemoveCartItemCommand(productId), CancellationToken.None);

        // Assert
        cart.Items.Should().BeEmpty();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Cart_Item_Cannot_Be_Removed_When_It_Does_Not_Exist()
    {
        // Arrange
        var cart = new Cart(_userId);
        cart.AddItem(Guid.NewGuid(), 2);

        SetupCartFound(cart);

        // Act
        var act = () => _sut.Handle(new RemoveCartItemCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
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
}
