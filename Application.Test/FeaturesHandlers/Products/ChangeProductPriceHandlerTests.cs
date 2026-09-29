using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Products.Commands.ChangeProductPrice;
using Domain.Entities.Catalog;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Products;

public class ChangeProductPriceHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ChangeProductPriceHandler _sut;

    public ChangeProductPriceHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new ChangeProductPriceHandler(
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task The_Price_Of_A_Product_Can_Be_Changed()
    {
        // Arrange
        var product = CreateExistingProduct();
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _sut.Handle(new ChangeProductPriceCommand(product.Id, 1250m), CancellationToken.None);

        // Assert
        product.Price.Should().Be(1250m);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task The_Price_Cannot_Be_Changed_When_The_Product_Does_Not_Exist()
    {
        // Arrange
        var command = new ChangeProductPriceCommand(Guid.NewGuid(), 1250m);

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
