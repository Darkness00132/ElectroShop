using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Discounts.Commands.DeleteDiscount;
using Domain.Entities.Catalog;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Discounts;

public class DeleteDiscountCommandHandlerTests
{
    private readonly Mock<IRepository<Discount>> _discountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteDiscountHandler _sut;

    public DeleteDiscountCommandHandlerTests()
    {
        _discountRepositoryMock = new Mock<IRepository<Discount>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new DeleteDiscountHandler(
            _discountRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task A_Discount_Can_Be_Deleted()
    {
        // Arrange
        var discount = CreateExistingDiscount();

        _discountRepositoryMock
            .Setup(x => x.GetByIdAsync(discount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);

        // Act
        await _sut.Handle(new DeleteDiscountCommand(discount.Id), CancellationToken.None);

        // Assert
        _discountRepositoryMock.Verify(
            x => x.Remove(discount),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Discount_Cannot_Be_Deleted_When_It_Does_Not_Exist()
    {
        // Arrange
        var command = new DeleteDiscountCommand(Guid.NewGuid());

        _discountRepositoryMock
            .Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Discount?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _discountRepositoryMock.Verify(
            x => x.Remove(It.IsAny<Discount>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
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
