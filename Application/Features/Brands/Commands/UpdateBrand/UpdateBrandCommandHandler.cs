using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.Catalog;
using MediatR;

namespace Application.Features.Brands.Commands.UpdateBrand;

internal class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand>
{
    private readonly IRepository<Brand> _brandRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBrandCommandHandler(
        IRepository<Brand> brandRepository,
        IUnitOfWork unitOfWork)
    {
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateBrandCommand request,
        CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (brand is null)
            throw new NotFoundException(nameof(Brand), request.Id);

        var duplicatedBrand = await _brandRepository.SingleOrDefaultAsync(
            existingBrand =>
                existingBrand.Id != request.Id &&
                (
                    existingBrand.NameEn == request.NameEn ||
                    existingBrand.NameAr == request.NameAr
                ),
            cancellationToken);

        if (duplicatedBrand is not null)
            throw new ConflictException("A brand with the same English or Arabic name already exists.");

        brand.UpdateEnglishName(request.NameEn);
        brand.UpdateArabicName(request.NameAr);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
