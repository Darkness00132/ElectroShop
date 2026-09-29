using Application.Abstractions.Repositories;
using Domain.Entities.Catalog;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Discounts.Commands.CreateDiscount;

internal class CreateDiscountHandler : IRequestHandler<CreateDiscountCommand, Guid>
{
    private readonly IRepository<Discount> _discountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDiscountHandler(IRepository<Discount> discountRepository, IUnitOfWork unitOfWork)
    {
        _discountRepository = discountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateDiscountCommand request, CancellationToken cancellationToken)
    {
        var discount = new Discount(request.Name,
            request.DiscountType,
            request.Value,
            new DateRange(request.StartDate, request.EndDate));

        if (!request.IsActive)
            discount.Deactivate();

        await _discountRepository.AddAsync(discount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return discount.Id;
    }
}
