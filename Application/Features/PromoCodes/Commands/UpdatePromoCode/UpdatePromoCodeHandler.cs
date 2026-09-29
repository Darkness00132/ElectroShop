using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.PromotionsAggregate;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.PromoCodes.Commands.UpdatePromoCode;

internal class UpdatePromoCodeHandler : IRequestHandler<UpdatePromoCodeCommand>
{
    private readonly IRepository<PromoCode> _promoCodeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePromoCodeHandler(IRepository<PromoCode> promoCodeRepository, IUnitOfWork unitOfWork)
    {
        _promoCodeRepository = promoCodeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdatePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var promoCode = await _promoCodeRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(PromoCode), request.Id);

        var code = request.Code ?? promoCode.Code;
        var normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode != promoCode.Code &&
            await _promoCodeRepository.ExistsAsync(p => p.Code == normalizedCode, cancellationToken)) {
            throw new ConflictException("A promo code with the same code already exists.");
        }

        promoCode.Update(
            code,
            request.DiscountType ?? promoCode.DiscountType,
            request.Value ?? promoCode.Value,
            request.MinimumOrder ?? promoCode.MinimumOrder,
            new DateRange(
                request.StartDate ?? promoCode.StartDate,
                request.EndDate ?? promoCode.ExpirationDate),
            request.UsageLimit ?? promoCode.UsageLimit);

        if (request.IsActive is not null) {
            if (request.IsActive.Value)
                promoCode.Activate();
            else
                promoCode.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
