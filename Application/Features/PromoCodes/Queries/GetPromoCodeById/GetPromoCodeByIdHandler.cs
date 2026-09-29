using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.PromoCodes.Dtos;
using Domain.Entities.PromotionsAggregate;
using MediatR;

namespace Application.Features.PromoCodes.Queries.GetPromoCodeById;

internal class GetPromoCodeByIdHandler : IRequestHandler<GetPromoCodeByIdQuery, PromoCodeDto>
{
    private readonly IRepository<PromoCode> _promoCodeRepository;

    public GetPromoCodeByIdHandler(IRepository<PromoCode> promoCodeRepository)
    {
        _promoCodeRepository = promoCodeRepository;
    }

    public async Task<PromoCodeDto> Handle(GetPromoCodeByIdQuery request, CancellationToken cancellationToken)
    {
        var promoCode = await _promoCodeRepository.ProjectToSingleOrDefaultAsync<PromoCodeDto>(
            p => p.Id == request.Id,
            cancellationToken);

        if (promoCode is null)
            throw new NotFoundException(nameof(PromoCode), request.Id);

        return promoCode;
    }
}
