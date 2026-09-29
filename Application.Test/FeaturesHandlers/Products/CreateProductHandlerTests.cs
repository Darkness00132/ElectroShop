using System.Linq.Expressions;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Files;
using Application.Constants;
using Application.Exceptions;
using Application.Features.Products.Commands.CreateProduct;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Products;

public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IRepository<Brand>> _brandRepositoryMock;
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly Mock<IRepository<Discount>> _discountRepositoryMock;
    private readonly Mock<IRepository<Inventory>> _inventoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IImageManipulationService> _imageManipulationServiceMock;
    private readonly CreateProductHandler _sut;

    public CreateProductHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _brandRepositoryMock = new Mock<IRepository<Brand>>();
        _categoryRepositoryMock = new Mock<IRepository<Category>>();
        _discountRepositoryMock = new Mock<IRepository<Discount>>();
        _inventoryRepositoryMock = new Mock<IRepository<Inventory>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _storageServiceMock = new Mock<IStorageService>();
        _imageManipulationServiceMock = new Mock<IImageManipulationService>();

        _sut = new CreateProductHandler(
            _productRepositoryMock.Object,
            _brandRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _discountRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _storageServiceMock.Object,
            _imageManipulationServiceMock.Object);
    }

    [Fact]
    public async Task A_Product_Can_Be_Created()
    {
        // Arrange
        var command = CreateValidCommand() with {
            IsActive = true,
            Attributes = new Dictionary<string, string> { ["RAM"] = "16GB" }
        };

        Product? addedProduct = null;
        Inventory? addedInventory = null;

        SetupCategoryExists();
        SetupBrandExists();
        SetupImageUpload("products/laptop.webp");

        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => addedProduct = product);

        _inventoryRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Inventory>(), It.IsAny<CancellationToken>()))
            .Callback<Inventory, CancellationToken>((inventory, _) => addedInventory = inventory);

        // Act
        var productId = await _sut.Handle(command, CancellationToken.None);

        // Assert
        productId.Should().NotBeEmpty();

        addedProduct.Should().NotBeNull();
        addedProduct!.NameEn.Should().Be(command.NameEn);
        addedProduct.NameAr.Should().Be(command.NameAr);
        addedProduct.SKU.Should().Be(command.SKU);
        addedProduct.Price.Should().Be(command.Price);
        addedProduct.IsActive.Should().BeTrue();
        addedProduct.Images.Should().ContainSingle(image => image.ImageKey == "products/laptop.webp");
        addedProduct.Attributes.Should().ContainSingle(attribute => attribute.Name == "RAM" && attribute.Value == "16GB");

        addedInventory.Should().NotBeNull();
        addedInventory!.ProductId.Should().Be(addedProduct.Id);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Product_Is_Created_With_An_Assigned_Discount()
    {
        // Arrange
        var discount = new Discount(
            "Summer Sale",
            Domain.Enums.DiscountType.Percentage,
            15,
            new Domain.ValueObjects.DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));

        var command = CreateValidCommand() with { DiscountId = discount.Id };

        Product? addedProduct = null;

        SetupCategoryExists();
        SetupBrandExists();
        SetupDiscountFound(discount);
        SetupImageUpload("products/laptop.webp");

        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => addedProduct = product);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        addedProduct!.DiscountId.Should().Be(discount.Id);
    }

    [Fact]
    public async Task A_Product_Cannot_Be_Created_When_The_Category_Does_Not_Exist()
    {
        // Arrange
        var command = CreateValidCommand();

        SetupCategoryMissing();

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _productRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _storageServiceMock.Verify(
            x => x.UploadManyAsync(It.IsAny<IEnumerable<FileDto>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task A_Product_Cannot_Be_Created_When_The_Brand_Does_Not_Exist()
    {
        // Arrange
        var command = CreateValidCommand();

        SetupCategoryExists();
        SetupBrandMissing();

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _productRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task A_Product_Cannot_Be_Created_When_The_Discount_Does_Not_Exist()
    {
        // Arrange
        var command = CreateValidCommand() with { DiscountId = Guid.NewGuid() };

        SetupCategoryExists();
        SetupBrandExists();
        SetupDiscountMissing();

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _productRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task The_Uploaded_Images_Are_Deleted_When_Saving_The_Product_Fails()
    {
        // Arrange
        var command = CreateValidCommand();

        SetupCategoryExists();
        SetupBrandExists();
        SetupImageUpload("products/laptop.webp");

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        _storageServiceMock.Verify(
            x => x.DeleteManyAsync(It.Is<IEnumerable<string>>(keys => keys.Contains("products/laptop.webp"))),
            Times.Once);
    }

    private static CreateProductCommand CreateValidCommand()
    {
        return new CreateProductCommand(
            "Gaming Laptop",
            "لابتوب ألعاب",
            "A laptop for gaming.",
            "لابتوب للألعاب.",
            "SKU-001",
            1000m,
            IsActive: false,
            CategoryId: Guid.NewGuid(),
            BrandId: Guid.NewGuid(),
            DiscountId: null,
            Attributes: null,
            Images: [CreateImageFile("laptop.jpg")]);
    }

    private static FileDto CreateImageFile(string fileName)
    {
        return new FileDto(
            fileName,
            "image/jpeg",
            10,
            new MemoryStream());
    }

    private void SetupCategoryExists()
    {
        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private void SetupCategoryMissing()
    {
        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupBrandExists()
    {
        _brandRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private void SetupBrandMissing()
    {
        _brandRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupDiscountFound(Discount discount)
    {
        _discountRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Discount, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);
    }

    private void SetupDiscountMissing()
    {
        _discountRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Discount, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Discount?)null);
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
