using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.Catalog;
using MediatR;

namespace Application.Features.Discounts.Commands.DeleteDiscount;

internal class DeleteDiscountHandler : IRequestHandler<DeleteDiscountCommand>
{
    private readonly IRepository<Discount> _discountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDiscountHandler(IRepository<Discount> discountRepository, IUnitOfWork unitOfWork)
    {
        _discountRepository = discountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteDiscountCommand request, CancellationToken cancellationToken)
    {
        var discount = await _discountRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (discount is null)
            throw new NotFoundException(nameof(Discount), request.Id);

        _discountRepository.Remove(discount);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
