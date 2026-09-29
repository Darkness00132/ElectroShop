using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Products.Commands.AssignProductDiscount;
using Domain.Entities.Catalog;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Products;

public class AssignProductDiscountHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IRepository<Discount>> _discountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AssignProductDiscountHandler _sut;

    public AssignProductDiscountHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _discountRepositoryMock = new Mock<IRepository<Discount>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new AssignProductDiscountHandler(
            _productRepositoryMock.Object,
            _discountRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task A_Discount_Can_Be_Assigned_To_A_Product()
    {
        // Arrange
        var product = CreateExistingProduct();
        var discount = CreateExistingDiscount();

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _discountRepositoryMock
            .Setup(x => x.GetByIdAsync(discount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);

        // Act
        await _sut.Handle(new AssignProductDiscountCommand(product.Id, discount.Id), CancellationToken.None);

        // Assert
        product.DiscountId.Should().Be(discount.Id);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Discount_Cannot_Be_Assigned_When_The_Product_Does_Not_Exist()
    {
        // Arrange
        var command = new AssignProductDiscountCommand(Guid.NewGuid(), Guid.NewGuid());

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task A_Discount_Cannot_Be_Assigned_When_The_Discount_Does_Not_Exist()
    {
        // Arrange
        var product = CreateExistingProduct();
        var command = new AssignProductDiscountCommand(product.Id, Guid.NewGuid());

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _discountRepositoryMock
            .Setup(x => x.GetByIdAsync(command.DiscountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Discount?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        product.DiscountId.Should().BeNull();

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

    private static Discount CreateExistingDiscount()
    {
        return new Discount(
            "Summer Sale",
            DiscountType.Percentage,
            15,
            new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));
    }
}
