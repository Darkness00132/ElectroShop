using System.Linq.Expressions;
using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Brands.Commands.CreateBrand;
using Domain.Entities.Catalog;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Brands;

public class CreateBrandCommandHandlerTests
{
    private readonly Mock<IRepository<Brand>> _brandRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateBrandCommandHandler _sut;

    public CreateBrandCommandHandlerTests()
    {
        _brandRepositoryMock = new Mock<IRepository<Brand>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new CreateBrandCommandHandler(
            _brandRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task A_New_Brand_Can_Be_Created()
    {
        // Arrange
        Brand? addedBrand = null;
        var command = new CreateBrandCommand("Samsung", "سامسونج");

        _brandRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Brand, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Brand?)null);

        _brandRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
            .Callback<Brand, CancellationToken>((brand, _) => addedBrand = brand);

        // Act
        var brandId = await _sut.Handle(command, CancellationToken.None);

        // Assert
        brandId.Should().NotBeEmpty();
        addedBrand.Should().NotBeNull();
        addedBrand.NameEn.Should().Be("Samsung");
        addedBrand.NameAr.Should().Be("سامسونج");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Brand_Cannot_Be_Created_When_Another_Brand_Already_Uses_The_Same_Name()
    {
        // Arrange
        var command = new CreateBrandCommand("Samsung", "سامسونج");

        _brandRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Brand, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Brand("Samsung", "existing"));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        _brandRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
