using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Constants;
using Application.Exceptions;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

internal class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IRepository<Brand> _brandRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Discount> _discountRepository;
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly IImageManipulationService _imageManipulationService;

    public CreateProductHandler(IProductRepository productRepository, IRepository<Brand> brandRepository, IRepository<Category> categoryRepository, IRepository<Discount> discountRepository, IRepository<Inventory> inventoryRepository, IUnitOfWork unitOfWork, IStorageService storageService, IImageManipulationService imageManipulationService)
    {
        _productRepository = productRepository;
        _brandRepository = brandRepository;
        _categoryRepository = categoryRepository;
        _discountRepository = discountRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _imageManipulationService = imageManipulationService;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (!await _categoryRepository.ExistsAsync(x => x.Id == request.CategoryId, cancellationToken))
            throw new NotFoundException(nameof(Category), request.CategoryId);

        if (!await _brandRepository.ExistsAsync(x => x.Id == request.BrandId, cancellationToken))
            throw new NotFoundException(nameof(Brand), request.BrandId);

        var product = new Product(request.NameEn,
            request.NameAr,
            request.DescriptionEn,
            request.DescriptionAr,
            request.SKU,
            request.Price,
            request.CategoryId,
            request.BrandId);

        if (request.IsActive)
            product.Activate();

        if (request.DiscountId is not null) {
            var discount = await _discountRepository
                .SingleOrDefaultAsync(x => x.Id == request.DiscountId, cancellationToken)
                ?? throw new NotFoundException(nameof(Discount), request.DiscountId);

            product.AssignDiscount(discount);
        }

        if (request.Attributes is not null) {
            foreach (var attribute in request.Attributes)
                product.AddAttribute(attribute.Key, attribute.Value);
        }

        var resizedImages = await Task.WhenAll(
            request.Images.Select(image =>
                _imageManipulationService.ResizeImageAsync(image, ImageType.Product, cancellationToken)));

        var uploadedImageKeys = (await _storageService
            .UploadManyAsync(resizedImages, FileDestination.Products, cancellationToken))
            .ToList();

        foreach (var imageKey in uploadedImageKeys)
            product.AddImage(imageKey);

        await _inventoryRepository.AddAsync(new Inventory(product.Id, 0, 0), cancellationToken);
        await _productRepository.AddAsync(product, cancellationToken);

        try {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch {
            await _storageService.DeleteManyAsync(uploadedImageKeys);
            throw;
        }

        return product.Id;
    }
}
