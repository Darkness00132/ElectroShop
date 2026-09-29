using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.Catalog;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Discounts.Commands.UpdateDiscount;

internal sealed class UpdateDiscountHandler
    : IRequestHandler<UpdateDiscountCommand>
{
    private readonly IRepository<Discount> _discountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDiscountHandler(
        IRepository<Discount> discountRepository,
        IUnitOfWork unitOfWork)
    {
        _discountRepository = discountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateDiscountCommand request,
        CancellationToken cancellationToken)
    {
        var discount = await _discountRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (discount is null) {
            throw new NotFoundException(nameof(Discount), request.Id);
        }

        var name = request.Name ?? discount.Name;

        var discountType =
            request.DiscountType ?? discount.DiscountType;

        var value =
            request.Value ?? discount.Value;

        var validityPeriod = new DateRange(
            request.StartDate ?? discount.StartDate,
            request.EndDate ?? discount.EndDate);

        discount.UpdateDetails(
            name,
            discountType,
            value,
            validityPeriod);

        if (request.IsActive is not null) {
            if (request.IsActive.Value) {
                discount.Activate();
            }
            else {
                discount.Deactivate();
            }
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
