using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Constants;
using Application.Exceptions;
using Domain.Entities.Catalog;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Categories.Commands.UpdateCategory;

internal class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IImageManipulationService _imageManipulationService;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(IRepository<Category> categoryRepository, IImageManipulationService imageManipulationService, IStorageService storageService, IUnitOfWork unitOfWork, ILogger<UpdateCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _imageManipulationService = imageManipulationService;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (category is null)
            throw new NotFoundException(nameof(Category), request.Id);

        var duplicatedCategory = await _categoryRepository.SingleOrDefaultAsync(
            existingCategory =>
                existingCategory.Id != request.Id &&
                (
                    existingCategory.NameEn == request.NameEn ||
                    existingCategory.NameAr == request.NameAr
                ),
            cancellationToken);

        if (duplicatedCategory is not null)
            throw new ConflictException("A category with the same English or Arabic name already exists.");

        string? newImageKey = null;
        var previousImageKey = category.ImageKey;

        if (request.NewImage is not null && request.NewImage.Length > 0) {
            var manipulatedImage = await _imageManipulationService.ResizeImageAsync(request.NewImage, ImageType.Category, cancellationToken);
            newImageKey = await _storageService.UploadAsync(manipulatedImage, FileDestination.Categories, cancellationToken);
        }

        category.UpdateDetails(
            nameEn: request.NameEn,
            nameAr: request.NameAr,
            descriptionEn: request.DescriptionEn,
            descriptionAr: request.DescriptionAr);

        category.UpdateImageKey(newImageKey ?? category.ImageKey);

        try {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch {
            if (newImageKey is not null)
                await _storageService.DeleteAsync(newImageKey, cancellationToken);
            throw;
        }

        // The update is already committed, so a failure to clean up the replaced image must not fail the request.
        if (newImageKey is not null && previousImageKey != newImageKey) {
            try {
                await _storageService.DeleteAsync(previousImageKey, cancellationToken);
            }
            catch (Exception exception) {
                _logger.LogWarning(
                    exception,
                    "Failed to delete the replaced category image. ImageKey: {ImageKey}",
                    previousImageKey);
            }
        }
    }
}
