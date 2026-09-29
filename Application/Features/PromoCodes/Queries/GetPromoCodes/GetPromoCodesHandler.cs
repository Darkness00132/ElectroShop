using Application.Abstractions.Repositories;
using Application.Common.Pagination;
using Application.Features.PromoCodes.Dtos;
using Domain.Entities.PromotionsAggregate;
using MediatR;

namespace Application.Features.PromoCodes.Queries.GetPromoCodes;

internal class GetPromoCodesHandler : IRequestHandler<GetPromoCodesQuery, PagedResult<PromoCodeDto>>
{
    private readonly IRepository<PromoCode> _promoCodeRepository;

    public GetPromoCodesHandler(IRepository<PromoCode> promoCodeRepository)
    {
        _promoCodeRepository = promoCodeRepository;
    }

    public async Task<PagedResult<PromoCodeDto>> Handle(GetPromoCodesQuery request, CancellationToken cancellationToken)
    {
        return await _promoCodeRepository.ProjectToPagedAsync<PromoCodeDto>(
            request.Pagination,
            orderBy: promoCode => promoCode.StartDate,
            cancellationToken: cancellationToken);
    }
}
