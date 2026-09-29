using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.PromotionsAggregate;
using MediatR;

namespace Application.Features.PromoCodes.Commands.ActivatePromoCode;

internal class ActivatePromoCodeHandler : IRequestHandler<ActivatePromoCodeCommand>
{
    private readonly IRepository<PromoCode> _promoCodeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivatePromoCodeHandler(IRepository<PromoCode> promoCodeRepository, IUnitOfWork unitOfWork)
    {
        _promoCodeRepository = promoCodeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivatePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var promoCode = await _promoCodeRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(PromoCode), request.Id);

        promoCode.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
