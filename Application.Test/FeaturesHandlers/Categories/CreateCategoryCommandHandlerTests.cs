using System.Linq.Expressions;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Files;
using Application.Constants;
using Application.Exceptions;
using Application.Features.Categories.Commands.CreateCategory;
using Domain.Entities.Catalog;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Categories;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IImageManipulationService> _imageManipulationServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateCategoryCommandHandler _sut;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IRepository<Category>>();
        _storageServiceMock = new Mock<IStorageService>();
        _imageManipulationServiceMock = new Mock<IImageManipulationService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new CreateCategoryCommandHandler(
            _categoryRepositoryMock.Object,
            _storageServiceMock.Object,
            _imageManipulationServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task A_New_Category_Can_Be_Created()
    {
        // Arrange
        var command = CreateValidCommand();
        Category? addedCategory = null;

        SetupNameAvailable();
        SetupImageUpload("categories/image.jpg");

        _categoryRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Callback<Category, CancellationToken>((category, _) => addedCategory = category);

        // Act
        var categoryId = await _sut.Handle(command, CancellationToken.None);

        // Assert
        categoryId.Should().NotBeEmpty();
        addedCategory.Should().NotBeNull();
        addedCategory.NameEn.Should().Be(command.NameEn);
        addedCategory.NameAr.Should().Be(command.NameAr);
        addedCategory.ImageKey.Should().Be("categories/image.jpg");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Category_Cannot_Be_Created_When_Another_Category_Already_Uses_The_Same_Name()
    {
        // Arrange
        var command = CreateValidCommand();

        SetupNameTaken();

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        _categoryRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _storageServiceMock.Verify(
            x => x.UploadAsync(It.IsAny<FileDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task The_UploadedImage_Is_Deleted_When_Saving_The_New_Category_Fails()
    {
        // Arrange
        var command = CreateValidCommand();

        SetupNameAvailable();
        SetupImageUpload("categories/image.jpg");

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        _storageServiceMock.Verify(
            x => x.DeleteAsync("categories/image.jpg", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CreateCategoryCommand CreateValidCommand()
    {
        return new CreateCategoryCommand(
            "Laptops",
            "لابتوبات",
            "Portable computers.",
            "أجهزة كمبيوتر محمولة.",
            CreateImageFile("laptop.jpg"));
    }

    private static FileDto CreateImageFile(string fileName)
    {
        return new FileDto(
            fileName,
            "image/jpeg",
            10,
            new MemoryStream());
    }

    private void SetupNameAvailable()
    {
        _categoryRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);
    }

    private void SetupNameTaken()
    {
        _categoryRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category("Laptops", "لابتوبات", "categories/existing.jpg"));
    }

    private void SetupImageUpload(string imageKey)
    {
        _imageManipulationServiceMock
            .Setup(x => x.ResizeImageAsync(
                It.IsAny<FileDto>(),
                It.Is<ImageType>(type => type == ImageType.Category),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileDto file, ImageType _, CancellationToken _) => file);

        _storageServiceMock
            .Setup(x => x.UploadAsync(
                It.IsAny<FileDto>(),
                FileDestination.Categories,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageKey);
    }
}
