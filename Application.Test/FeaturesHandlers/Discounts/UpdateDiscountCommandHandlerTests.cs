using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Discounts.Commands.UpdateDiscount;
using Domain.Entities.Catalog;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Discounts;

public class UpdateDiscountCommandHandlerTests
{
    private static readonly DateOnly ValidFrom = new(2026, 1, 1);
    private static readonly DateOnly ValidTo = new(2026, 12, 31);

    private readonly Mock<IRepository<Discount>> _discountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateDiscountHandler _sut;

    public UpdateDiscountCommandHandlerTests()
    {
        _discountRepositoryMock = new Mock<IRepository<Discount>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new UpdateDiscountHandler(
            _discountRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task A_Discount_Can_Be_Updated_With_New_Details()
    {
        // Arrange
        var discount = CreateExistingDiscount();
        var command = new UpdateDiscountCommand(
            discount.Id,
            "Black Friday",
            DiscountType.FixedAmount,
            50,
            new DateOnly(2026, 11, 25),
            new DateOnly(2026, 11, 30),
            IsActive: null);

        SetupDiscountFound(discount);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        discount.Name.Should().Be("Black Friday");
        discount.DiscountType.Should().Be(DiscountType.FixedAmount);
        discount.Value.Should().Be(50);
        discount.StartDate.Should().Be(new DateOnly(2026, 11, 25));
        discount.EndDate.Should().Be(new DateOnly(2026, 11, 30));

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Unspecified_Details_Keep_Their_Current_Values()
    {
        // Arrange
        var discount = CreateExistingDiscount();
        var command = new UpdateDiscountCommand(
            discount.Id,
            "Black Friday",
            DiscountType: null,
            Value: null,
            StartDate: null,
            EndDate: null,
            IsActive: null);

        SetupDiscountFound(discount);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        discount.Name.Should().Be("Black Friday");
        discount.DiscountType.Should().Be(DiscountType.Percentage);
        discount.Value.Should().Be(15);
        discount.StartDate.Should().Be(ValidFrom);
        discount.EndDate.Should().Be(ValidTo);
        discount.IsVisible.Should().BeTrue();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Discount_Can_Be_Deactivated()
    {
        // Arrange
        var discount = CreateExistingDiscount();
        var command = new UpdateDiscountCommand(
            discount.Id,
            Name: null,
            DiscountType: null,
            Value: null,
            StartDate: null,
            EndDate: null,
            IsActive: false);

        SetupDiscountFound(discount);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        discount.IsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task An_Inactive_Discount_Can_Be_Activated()
    {
        // Arrange
        var discount = CreateExistingDiscount();
        discount.Deactivate();

        var command = new UpdateDiscountCommand(
            discount.Id,
            Name: null,
            DiscountType: null,
            Value: null,
            StartDate: null,
            EndDate: null,
            IsActive: true);

        SetupDiscountFound(discount);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        discount.IsVisible.Should().BeTrue();
    }

    [Fact]
    public async Task A_Discount_Cannot_Be_Updated_When_It_Does_Not_Exist()
    {
        // Arrange
        var command = new UpdateDiscountCommand(
            Guid.NewGuid(),
            "Black Friday",
            null,
            null,
            null,
            null,
            IsActive: null);

        _discountRepositoryMock
            .Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Discount?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

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
            new DateRange(ValidFrom, ValidTo));
    }

    private void SetupDiscountFound(Discount discount)
    {
        _discountRepositoryMock
            .Setup(x => x.GetByIdAsync(discount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);
    }
}
