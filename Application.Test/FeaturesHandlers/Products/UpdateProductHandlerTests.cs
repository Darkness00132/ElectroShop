using System.Linq.Expressions;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Files;
using Application.Constants;
using Application.Exceptions;
using Application.Features.Products.Commands.UpdateProduct;
using Domain.Entities.Catalog;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.Test.FeaturesHandlers.Products;

public class UpdateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IRepository<Brand>> _brandRepositoryMock;
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IImageManipulationService> _imageManipulationServiceMock;
    private readonly UpdateProductHandler _sut;

    public UpdateProductHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _brandRepositoryMock = new Mock<IRepository<Brand>>();
        _categoryRepositoryMock = new Mock<IRepository<Category>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _storageServiceMock = new Mock<IStorageService>();
        _imageManipulationServiceMock = new Mock<IImageManipulationService>();

        _sut = new UpdateProductHandler(
            _productRepositoryMock.Object,
            _brandRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _imageManipulationServiceMock.Object,
            _storageServiceMock.Object,
            NullLogger<UpdateProductHandler>.Instance);
    }

    [Fact]
    public async Task A_Product_Can_Be_Updated_With_New_Details()
    {
        // Arrange
        var product = CreateExistingProduct();
        var command = new UpdateProductCommand(
            product.Id,
            "Gaming Laptop Pro",
            null,
            "SKU-002",
            null,
            null,
            Price: 1250m,
            CategoryId: null,
            BrandId: null,
            DeletedImages: null,
            NewImages: null,
            Attributes: null);

        SetupProductFound(product);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        product.NameEn.Should().Be("Gaming Laptop Pro");
        product.SKU.Should().Be("SKU-002");
        product.Price.Should().Be(1250m);
        product.NameAr.Should().Be("لابتوب ألعاب");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Removed_Images_Are_Deleted_From_Storage_After_The_Update_Is_Saved()
    {
        // Arrange
        var product = CreateExistingProduct();
        var command = new UpdateProductCommand(
            product.Id,
            null, null, null, null, null,
            Price: null,
            CategoryId: null,
            BrandId: null,
            DeletedImages: ["products/old.webp"],
            NewImages: null,
            Attributes: null);

        SetupProductFound(product);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        product.Images.Should().BeEmpty();

        _storageServiceMock.Verify(
            x => x.DeleteManyAsync(It.Is<IEnumerable<string>>(keys => keys.Contains("products/old.webp"))),
            Times.Once);
    }

    [Fact]
    public async Task A_Product_Gets_New_Images_When_They_Are_Provided()
    {
        // Arrange
        var product = CreateExistingProduct();
        var command = new UpdateProductCommand(
            product.Id,
            null, null, null, null, null,
            Price: null,
            CategoryId: null,
            BrandId: null,
            DeletedImages: null,
            NewImages: [CreateImageFile("new-laptop.jpg")],
            Attributes: null);

        SetupProductFound(product);
        SetupImageUpload("products/new-laptop.webp");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        product.Images.Should().Contain(image => image.ImageKey == "products/new-laptop.webp");
        product.Images.Should().HaveCount(2);
    }

    [Fact]
    public async Task The_Uploaded_Images_Are_Deleted_When_Saving_The_Update_Fails()
    {
        // Arrange
        var product = CreateExistingProduct();
        var command = new UpdateProductCommand(
            product.Id,
            null, null, null, null, null,
            Price: null,
            CategoryId: null,
            BrandId: null,
            DeletedImages: null,
            NewImages: [CreateImageFile("new-laptop.jpg")],
            Attributes: null);

        SetupProductFound(product);
        SetupImageUpload("products/new-laptop.webp");

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        _storageServiceMock.Verify(
            x => x.DeleteManyAsync(It.Is<IEnumerable<string>>(keys => keys.Contains("products/new-laptop.webp"))),
            Times.Once);
    }

    [Fact]
    public async Task The_Attributes_Are_Replaced_When_They_Are_Provided()
    {
        // Arrange
        var product = CreateExistingProduct();
        product.AddAttribute("RAM", "16GB");

        var command = new UpdateProductCommand(
            product.Id,
            null, null, null, null, null,
            Price: null,
            CategoryId: null,
            BrandId: null,
            DeletedImages: null,
            NewImages: null,
            Attributes: new Dictionary<string, string> { ["Color"] = "Black" });

        SetupProductFound(product);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        product.Attributes.Should().ContainSingle(attribute => attribute.Name == "Color" && attribute.Value == "Black");
    }

    [Fact]
    public async Task A_Product_Cannot_Be_Updated_When_It_Does_Not_Exist()
    {
        // Arrange
        var command = new UpdateProductCommand(
            Guid.NewGuid(),
            "Gaming Laptop Pro", null, null, null, null,
            Price: null,
            CategoryId: null,
            BrandId: null,
            DeletedImages: null,
            NewImages: null,
            Attributes: null);

        _productRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Product CreateExistingProduct()
    {
        var product = new Product(
            "Gaming Laptop",
            "لابتوب ألعاب",
            "A laptop for gaming.",
            "لابتوب للألعاب.",
            "SKU-001",
            1000m,
            Guid.NewGuid(),
            Guid.NewGuid());

        product.AddImage("products/old.webp");

        return product;
    }

    private static FileDto CreateImageFile(string fileName)
    {
        return new FileDto(
            fileName,
            "image/jpeg",
            10,
            new MemoryStream());
    }

    private void SetupProductFound(Product product)
    {
        _productRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(product);
    }

    private void SetupImageUpload(string imageKey)
    {
        _imageManipulationServiceMock
            .Setup(x => x.ResizeImageAsync(
                It.IsAny<FileDto>(),
                It.Is<ImageType>(type => type == ImageType.Product),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileDto file, ImageType _, CancellationToken _) => file);

        _storageServiceMock
            .Setup(x => x.UploadManyAsync(
                It.IsAny<IEnumerable<FileDto>>(),
                FileDestination.Products,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { imageKey });
    }
}
