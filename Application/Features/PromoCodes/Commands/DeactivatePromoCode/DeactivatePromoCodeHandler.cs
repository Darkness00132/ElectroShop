using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.PromotionsAggregate;
using MediatR;

namespace Application.Features.PromoCodes.Commands.DeactivatePromoCode;

internal class DeactivatePromoCodeHandler : IRequestHandler<DeactivatePromoCodeCommand>
{
    private readonly IRepository<PromoCode> _promoCodeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePromoCodeHandler(IRepository<PromoCode> promoCodeRepository, IUnitOfWork unitOfWork)
    {
        _promoCodeRepository = promoCodeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivatePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var promoCode = await _promoCodeRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(PromoCode), request.Id);

        promoCode.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
