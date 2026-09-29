using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.PromotionsAggregate;
using MediatR;

namespace Application.Features.PromoCodes.Commands.DeletePromoCode;

internal class DeletePromoCodeHandler : IRequestHandler<DeletePromoCodeCommand>
{
    private readonly IRepository<PromoCode> _promoCodeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePromoCodeHandler(IRepository<PromoCode> promoCodeRepository, IUnitOfWork unitOfWork)
    {
        _promoCodeRepository = promoCodeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeletePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var promoCode = await _promoCodeRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(PromoCode), request.Id);

        _promoCodeRepository.Remove(promoCode);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
