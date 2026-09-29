using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Products.Commands.AssignProductDiscount;
using Application.Features.Products.Commands.RemoveProductDiscount;
using Domain.Entities.Catalog;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Products;

public class RemoveProductDiscountHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveProductDiscountHandler _sut;

    public RemoveProductDiscountHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new RemoveProductDiscountHandler(
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task A_Discount_Can_Be_Removed_From_A_Product()
    {
        // Arrange
        var product = CreateExistingProductWithDiscount();
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _sut.Handle(new RemoveProductDiscountCommand(product.Id), CancellationToken.None);

        // Assert
        product.DiscountId.Should().BeNull();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Discount_Cannot_Be_Removed_When_The_Product_Does_Not_Exist()
    {
        // Arrange
        var command = new RemoveProductDiscountCommand(Guid.NewGuid());

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

    private static Product CreateExistingProductWithDiscount()
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

        var discount = new Discount(
            "Summer Sale",
            DiscountType.Percentage,
            15,
            new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));

        product.AssignDiscount(discount);

        return product;
    }
}
