using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Brands.Queries.GetBrandById;
using Domain.Entities.Catalog;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Brands;

public class GetBrandByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Brand>> _brandRepositoryMock;
    private readonly GetBrandByIdQueryHandler _sut;

    public GetBrandByIdQueryHandlerTests()
    {
        _brandRepositoryMock = new Mock<IRepository<Brand>>();

        _sut = new GetBrandByIdQueryHandler(_brandRepositoryMock.Object);
    }

    [Fact]
    public async Task A_Brand_Can_Be_Retrieved_By_Its_Id()
    {
        // Arrange
        var brand = new Brand("Samsung", "سامسونج");
        var query = new GetBrandByIdQuery(brand.Id);

        _brandRepositoryMock
            .Setup(x => x.GetByIdAsync(brand.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(brand);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Id.Should().Be(brand.Id);
        result.NameEn.Should().Be("Samsung");
        result.NameAr.Should().Be("سامسونج");
    }

    [Fact]
    public async Task A_Brand_Cannot_Be_Retrieved_When_It_Does_Not_Exist()
    {
        // Arrange
        var query = new GetBrandByIdQuery(Guid.NewGuid());

        _brandRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Brand?)null);

        // Act
        var act = () => _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
