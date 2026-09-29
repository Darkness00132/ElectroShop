using Application.Features.PromoCodes.Dtos;
using AutoMapper;
using Domain.Entities.PromotionsAggregate;

namespace Application.Features.PromoCodes;

internal class PromoCodeMapping : Profile
{
    public PromoCodeMapping()
    {
        CreateMap<PromoCode, PromoCodeDto>()
            .ForMember(d => d.StartDate, o => o.MapFrom(s => s.ValidityPeriod.StartDate))
            .ForMember(d => d.EndDate, o => o.MapFrom(s => s.ValidityPeriod.EndDate));
    }
}
