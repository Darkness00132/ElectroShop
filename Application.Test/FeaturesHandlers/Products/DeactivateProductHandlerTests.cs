using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Products.Commands.DeactivateProduct;
using Domain.Entities.Catalog;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Products;

public class DeactivateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeactivateProductHandler _sut;

    public DeactivateProductHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new DeactivateProductHandler(
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task A_Product_Can_Be_Deactivated()
    {
        // Arrange
        var product = CreateExistingProduct();
        product.Activate();

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _sut.Handle(new DeactivateProductCommand(product.Id), CancellationToken.None);

        // Assert
        product.IsActive.Should().BeFalse();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Product_Cannot_Be_Deactivated_When_It_Does_Not_Exist()
    {
        // Arrange
        var command = new DeactivateProductCommand(Guid.NewGuid());

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

    private static Product CreateExistingProduct()
    {
        return new Product(
            "Gaming Laptop",
            "لابتوب ألعاب",
            "A laptop for gaming.",
            "لابتوب للألعاب.",
            "SKU-001",
            1000m,
            Guid.NewGuid(),
            Guid.NewGuid());
    }
}
