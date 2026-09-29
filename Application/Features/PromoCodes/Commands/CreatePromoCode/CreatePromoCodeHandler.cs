using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.PromotionsAggregate;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.PromoCodes.Commands.CreatePromoCode;

internal class CreatePromoCodeHandler : IRequestHandler<CreatePromoCodeCommand, Guid>
{
    private readonly IRepository<PromoCode> _promoCodeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePromoCodeHandler(IRepository<PromoCode> promoCodeRepository, IUnitOfWork unitOfWork)
    {
        _promoCodeRepository = promoCodeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreatePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        if (await _promoCodeRepository.ExistsAsync(p => p.Code == normalizedCode, cancellationToken))
            throw new ConflictException("A promo code with the same code already exists.");

        var promoCode = new PromoCode(
            request.Code,
            request.DiscountType,
            request.Value,
            request.MinimumOrder,
            new DateRange(request.StartDate, request.EndDate),
            request.UsageLimit);

        await _promoCodeRepository.AddAsync(promoCode, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return promoCode.Id;
    }
}
