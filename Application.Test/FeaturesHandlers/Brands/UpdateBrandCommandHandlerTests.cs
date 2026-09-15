using System.Linq.Expressions;
using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Brands.Commands.UpdateBrand;
using Domain.Entities.Catalog;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Brands;

public class UpdateBrandCommandHandlerTests
{
    private readonly Mock<IRepository<Brand>> _brandRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateBrandCommandHandler _sut;

    public UpdateBrandCommandHandlerTests()
    {
        _brandRepositoryMock = new Mock<IRepository<Brand>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new UpdateBrandCommandHandler(
            _brandRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task A_Brand_Name_Can_Be_Updated()
    {
        // Arrange
        var brand = new Brand("Samsung", "سامسونج");
        var command = CreateUpdateBrandCommand(brand.Id);

        _brandRepositoryMock
            .Setup(x => x.GetByIdAsync(
                brand.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(brand);

        _brandRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Brand, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Brand?)null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        brand.NameEn.Should().Be("LG");
        brand.NameAr.Should().Be("إل جي");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Brand_Cannot_Be_Updated_When_It_Does_Not_Exist()
    {
        // Arrange
        var command = CreateUpdateBrandCommand(Guid.NewGuid());

        _brandRepositoryMock
            .Setup(x => x.GetByIdAsync(
                command.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Brand?)null);

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task A_Brand_Cannot_Be_Updated_When_Another_Brand_Already_Uses_The_New_Name()
    {
        // Arrange
        var brand = new Brand("Samsung", "سامسونج");
        var command = CreateUpdateBrandCommand(brand.Id);

        _brandRepositoryMock
            .Setup(x => x.GetByIdAsync(
                brand.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(brand);

        _brandRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Brand, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Brand("LG", "existing"));

        // Act
        var act = () => _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        brand.NameEn.Should().Be("Samsung");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private UpdateBrandCommand CreateUpdateBrandCommand(Guid id)
    {
        return new UpdateBrandCommand(id, "LG", "إل جي");
    }
}
