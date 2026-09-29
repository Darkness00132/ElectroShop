using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Features.Carts.Commands.ClearCart;
using Domain.Entities.Carts;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Carts;

public class ClearCartHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    private readonly Mock<IRepository<Cart>> _cartRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly ClearCartHandler _sut;

    public ClearCartHandlerTests()
    {
        _cartRepositoryMock = new Mock<IRepository<Cart>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(_userId);

        _sut = new ClearCartHandler(
            _cartRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task The_Cart_Can_Be_Cleared()
    {
        // Arrange
        var cart = new Cart(_userId);
        cart.AddItem(Guid.NewGuid(), 1);
        cart.AddItem(Guid.NewGuid(), 2);

        SetupCartFound(cart);

        // Act
        await _sut.Handle(new ClearCartCommand(), CancellationToken.None);

        // Assert
        cart.Items.Should().BeEmpty();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Clearing_A_Missing_Cart_Is_A_No_Op()
    {
        // Arrange
        SetupCartMissing();

        // Act
        await _sut.Handle(new ClearCartCommand(), CancellationToken.None);

        // Assert
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
