using Application.Abstractions.Repositories;
using Application.Constants;
using Application.Features.Discounts.Commands.CreateDiscount;
using Domain.Entities.Catalog;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Discounts;

public class CreateDiscountCommandHandlerTests
{
    private readonly Mock<IRepository<Discount>> _discountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateDiscountHandler _sut;

    public CreateDiscountCommandHandlerTests()
    {
        _discountRepositoryMock = new Mock<IRepository<Discount>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new CreateDiscountHandler(
            _discountRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task A_Discount_Can_Be_Created()
    {
        // Arrange
        var command = new CreateDiscountCommand(
            "Summer Sale",
            DiscountType.Percentage,
            15,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31),
            IsActive: true);

        Discount? addedDiscount = null;

        _discountRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Discount>(), It.IsAny<CancellationToken>()))
            .Callback<Discount, CancellationToken>((discount, _) => addedDiscount = discount);

        // Act
        var discountId = await _sut.Handle(command, CancellationToken.None);

        // Assert
        discountId.Should().NotBeEmpty();

        addedDiscount.Should().NotBeNull();
        addedDiscount!.Name.Should().Be(command.Name);
        addedDiscount.DiscountType.Should().Be(DiscountType.Percentage);
        addedDiscount.Value.Should().Be(15);
        addedDiscount.StartDate.Should().Be(new DateOnly(2026, 7, 1));
        addedDiscount.EndDate.Should().Be(new DateOnly(2026, 7, 31));
        addedDiscount.IsVisible.Should().BeTrue();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Discount_Can_Be_Created_As_Inactive()
    {
        // Arrange
        var command = new CreateDiscountCommand(
            "Upcoming Sale",
            DiscountType.FixedAmount,
            50,
            new DateOnly(2026, 12, 1),
            new DateOnly(2026, 12, 31),
            IsActive: false);

        Discount? addedDiscount = null;

        _discountRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Discount>(), It.IsAny<CancellationToken>()))
            .Callback<Discount, CancellationToken>((discount, _) => addedDiscount = discount);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        addedDiscount.Should().NotBeNull();
        addedDiscount!.IsVisible.Should().BeFalse();
    }
}
