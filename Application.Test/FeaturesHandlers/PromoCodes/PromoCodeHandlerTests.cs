using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.PromoCodes.Commands.CreatePromoCode;
using Application.Features.PromoCodes.Commands.DeletePromoCode;
using Application.Features.PromoCodes.Commands.UpdatePromoCode;
using Domain.Entities.PromotionsAggregate;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.PromoCodes;

public class PromoCodeHandlerTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly Mock<IRepository<PromoCode>> _promoCodeRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public PromoCodeHandlerTests()
    {
        _promoCodeRepositoryMock = new Mock<IRepository<PromoCode>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task A_Promo_Code_Can_Be_Created()
    {
        // Arrange
        SetupCodeAvailable();

        PromoCode? addedPromoCode = null;

        _promoCodeRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<PromoCode>(), It.IsAny<CancellationToken>()))
            .Callback<PromoCode, CancellationToken>((promoCode, _) => addedPromoCode = promoCode);

        var handler = new CreatePromoCodeHandler(_promoCodeRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var promoCodeId = await handler.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        promoCodeId.Should().Be(addedPromoCode!.Id);
        addedPromoCode.Code.Should().Be("SAVE10");
        addedPromoCode.Value.Should().Be(10);
        addedPromoCode.MinimumOrder.Should().Be(200);
        addedPromoCode.UsageLimit.Should().Be(5);
        addedPromoCode.IsActive.Should().BeTrue();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Promo_Code_Cannot_Be_Created_When_The_Code_Already_Exists()
    {
        // Arrange
        SetupCodeTaken();

        var handler = new CreatePromoCodeHandler(_promoCodeRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        _promoCodeRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<PromoCode>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task A_Promo_Code_Can_Be_Partially_Updated()
    {
        // Arrange
        var promoCode = CreateExistingPromoCode();
        SetupPromoCodeFound(promoCode);
        SetupCodeAvailable();

        var command = new UpdatePromoCodeCommand(
            promoCode.Id,
            Code: null,
            DiscountType: null,
            Value: 20,
            MinimumOrder: null,
            StartDate: null,
            EndDate: null,
            UsageLimit: null,
            IsActive: false);

        var handler = new UpdatePromoCodeHandler(_promoCodeRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        promoCode.Value.Should().Be(20);
        promoCode.Code.Should().Be("SAVE10");
        promoCode.MinimumOrder.Should().Be(200);
        promoCode.IsActive.Should().BeFalse();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Promo_Code_Cannot_Be_Updated_When_It_Does_Not_Exist()
    {
        // Arrange
        _promoCodeRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PromoCode?)null);

        var handler = new UpdatePromoCodeHandler(_promoCodeRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new UpdatePromoCodeCommand(Guid.NewGuid(), null, null, null, null, null, null, null, null), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task A_Promo_Code_Can_Be_Deleted()
    {
        // Arrange
        var promoCode = CreateExistingPromoCode();
        SetupPromoCodeFound(promoCode);

        var handler = new DeletePromoCodeHandler(_promoCodeRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new DeletePromoCodeCommand(promoCode.Id), CancellationToken.None);

        // Assert
        _promoCodeRepositoryMock.Verify(
            x => x.Remove(promoCode),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CreatePromoCodeCommand CreateCommand()
    {
        return new CreatePromoCodeCommand(
            " save10 ",
            PromoDiscountType.Percentage,
            10,
            200,
            Today.AddDays(-1),
            Today.AddDays(30),
            UsageLimit: 5);
    }

    private static PromoCode CreateExistingPromoCode()
    {
        return new PromoCode(
            "SAVE10",
            PromoDiscountType.Percentage,
            10,
            200,
            new DateRange(Today.AddDays(-1), Today.AddDays(30)),
            usageLimit: 5);
    }

    private void SetupCodeAvailable()
    {
        _promoCodeRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PromoCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupCodeTaken()
    {
        _promoCodeRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PromoCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private void SetupPromoCodeFound(PromoCode promoCode)
    {
        _promoCodeRepositoryMock
            .Setup(x => x.GetByIdAsync(promoCode.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(promoCode);
    }
}
