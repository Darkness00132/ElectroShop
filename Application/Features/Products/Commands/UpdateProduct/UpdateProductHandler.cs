using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Constants;
using Application.Exceptions;
using Domain.Entities.Catalog;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Products.Commands.UpdateProduct;

internal class UpdateProductHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IRepository<Brand> _brandRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageManipulationService _imageManipulationService;
    private readonly IStorageService _storageService;
    private readonly ILogger<UpdateProductHandler> _logger;

    public UpdateProductHandler(IProductRepository productRepository, IRepository<Brand> brandRepository, IRepository<Category> categoryRepository, IUnitOfWork unitOfWork, IImageManipulationService imageManipulationService, IStorageService storageService, ILogger<UpdateProductHandler> logger)
    {
        _productRepository = productRepository;
        _brandRepository = brandRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _imageManipulationService = imageManipulationService;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository
            .SingleOrDefaultAsync(p => p.Id == request.Id,
            cancellationToken,
            p => p.Images, p => p.Attributes);

        if (product is null)
            throw new NotFoundException(nameof(Product), request.Id);

        if (request.CategoryId is not null &&
            !await _categoryRepository.ExistsAsync(x => x.Id == request.CategoryId, cancellationToken)) {
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        if (request.BrandId is not null &&
            !await _brandRepository.ExistsAsync(x => x.Id == request.BrandId, cancellationToken)) {
            throw new NotFoundException(nameof(Brand), request.BrandId);
        }

        product.UpdateDetails(
            request.NameEn ?? product.NameEn,
            request.NameAr ?? product.NameAr,
            request.DescriptionEn ?? product.DescriptionEn,
            request.DescriptionAr ?? product.DescriptionAr,
            request.SKU ?? product.SKU);

        if (request.Price is not null)
            product.ChangePrice(request.Price.Value);

        var removedImageKeys = RemoveImages(product, request.DeletedImages);

        var uploadedImageKeys = new List<string>();

        if (request.NewImages is not null && request.NewImages.Count > 0) {
            var resizedImages = await Task.WhenAll(
                request.NewImages.Select(image =>
                    _imageManipulationService.ResizeImageAsync(image, ImageType.Product, cancellationToken)));

            uploadedImageKeys = (await _storageService
                .UploadManyAsync(resizedImages, FileDestination.Products, cancellationToken))
                .ToList();

            foreach (var imageKey in uploadedImageKeys)
                product.AddImage(imageKey);
        }

        if (request.Attributes is not null)
            SyncAttributes(product, request.Attributes);

        try {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch {
            if (uploadedImageKeys.Count > 0)
                await _storageService.DeleteManyAsync(uploadedImageKeys);
            throw;
        }

        // The update is already committed, so a failure to clean up removed images must not fail the request.
        if (removedImageKeys.Count > 0) {
            try {
                await _storageService.DeleteManyAsync(removedImageKeys);
            }
            catch (Exception exception) {
                _logger.LogWarning(
                    exception,
                    "Failed to delete the removed product images. ImageKeys: {ImageKeys}",
                    removedImageKeys);
            }
        }
    }

    private static List<string> RemoveImages(Product product, List<string>? deletedImages)
    {
        var removedImageKeys = new List<string>();

        if (deletedImages is null)
            return removedImageKeys;

        foreach (var imageKey in deletedImages) {
            var image = product.Images
                .FirstOrDefault(x => x.ImageKey == imageKey.Trim());

            if (image is null)
                continue;

            product.RemoveImage(image.ImageKey);
            removedImageKeys.Add(image.ImageKey);
        }

        return removedImageKeys;
    }

    private static void SyncAttributes(Product product, Dictionary<string, string> attributes)
    {
        foreach (var removedName in product.Attributes
            .Where(existing => !attributes.ContainsKey(existing.Name))
            .Select(existing => existing.Name)
            .ToList()) {
            product.RemoveAttribute(removedName);
        }

        foreach (var attribute in attributes)
            product.AddAttribute(attribute.Key, attribute.Value);
    }
}
