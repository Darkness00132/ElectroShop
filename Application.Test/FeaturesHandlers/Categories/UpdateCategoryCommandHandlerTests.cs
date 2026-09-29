using System.Linq.Expressions;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Files;
using Application.Constants;
using Application.Exceptions;
using Application.Features.Categories.Commands.UpdateCategory;
using Domain.Entities.Catalog;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.Test.FeaturesHandlers.Categories;

public class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IImageManipulationService> _imageManipulationServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateCategoryCommandHandler _sut;

    public UpdateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IRepository<Category>>();
        _storageServiceMock = new Mock<IStorageService>();
        _imageManipulationServiceMock = new Mock<IImageManipulationService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new UpdateCategoryCommandHandler(
            _categoryRepositoryMock.Object,
            _imageManipulationServiceMock.Object,
            _storageServiceMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<UpdateCategoryCommandHandler>.Instance);
    }

    [Fact]
    public async Task A_Category_Can_Be_Updated_With_New_Details()
    {
        // Arrange
        var category = CreateExistingCategory();
        var command = CreateValidUpdateCommand(category);

        SetupCategoryFound(category);
        SetupNameAvailable();
        SetupImageUpload("categories/new-image.jpg");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        category.NameEn.Should().Be(command.NameEn);
        category.NameAr.Should().Be(command.NameAr);
        category.DescriptionEn.Should().Be(command.DescriptionEn);
        category.DescriptionAr.Should().Be(command.DescriptionAr);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Category_Keeps_Its_Current_Image_When_No_New_Image_Is_Provided()
    {
        // Arrange
        var category = CreateExistingCategory();
        var command = CreateValidUpdateCommand(category) with { NewImage = null };

        SetupCategoryFound(category);
        SetupNameAvailable();

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        category.ImageKey.Should().Be("categories/existing.jpg");

        _storageServiceMock.Verify(
            x => x.UploadAsync(It.IsAny<FileDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task A_Category_Gets_A_New_Image_When_One_Is_Provided()
    {
        // Arrange
        var category = CreateExistingCategory();
        var command = CreateValidUpdateCommand(category);

        SetupCategoryFound(category);
        SetupNameAvailable();
        SetupImageUpload("categories/new-image.jpg");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        category.ImageKey.Should().Be("categories/new-image.jpg");
    }

    [Fact]
    public async Task A_Category_Cannot_Be_Updated_When_It_Does_Not_Exist()
    {
        // Arrange
        var command = CreateValidUpdateCommand();

        SetupCategoryNotFound(command.Id);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task A_Category_Cannot_Be_Updated_When_Another_Category_Already_Uses_The_Same_Name()
    {
        // Arrange
        var category = CreateExistingCategory();
        var command = CreateValidUpdateCommand(category);

        SetupCategoryFound(category);
        SetupNameTaken();

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        category.NameEn.Should().Be("Laptops");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task The_Previous_Image_Is_Deleted_When_It_Is_Replaced_By_A_New_One()
    {
        // Arrange
        var category = CreateExistingCategory();
        var command = CreateValidUpdateCommand(category);

        SetupCategoryFound(category);
        SetupNameAvailable();
        SetupImageUpload("categories/new-image.jpg");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _storageServiceMock.Verify(
            x => x.DeleteAsync("categories/existing.jpg", It.IsAny<CancellationToken>()),
            Times.Once);

        _storageServiceMock.Verify(
            x => x.DeleteAsync("categories/new-image.jpg", It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task The_New_Image_Is_Deleted_When_Saving_The_Update_Fails()
    {
        // Arrange
        var category = CreateExistingCategory();
        var command = CreateValidUpdateCommand(category);

        SetupCategoryFound(category);
        SetupNameAvailable();
        SetupImageUpload("categories/new-image.jpg");

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        _storageServiceMock.Verify(
            x => x.DeleteAsync("categories/new-image.jpg", It.IsAny<CancellationToken>()),
            Times.Once);

        _storageServiceMock.Verify(
            x => x.DeleteAsync("categories/existing.jpg", It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Category CreateExistingCategory()
    {
        return new Category(
            "Laptops",
            "لابتوبات",
            "categories/existing.jpg",
            "Portable computers.",
            "أجهزة كمبيوتر محمولة.");
    }

    private static UpdateCategoryCommand CreateValidUpdateCommand(Category? category = null)
    {
        return new UpdateCategoryCommand(
            category?.Id ?? Guid.NewGuid(),
            "Gaming Laptops",
            "لابتوبات الألعاب",
            "High performance portable computers.",
            "أجهزة كمبيوتر محمولة عالية الأداء.",
            new FileDto(
                "new-image.jpg",
                "image/jpeg",
                10,
                new MemoryStream()));
    }

    private void SetupCategoryFound(Category category)
    {
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
    }

    private void SetupCategoryNotFound(Guid id)
    {
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);
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
            .ReturnsAsync(new Category("Gaming Laptops", "لابتوبات الألعاب", "categories/other.jpg"));
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
