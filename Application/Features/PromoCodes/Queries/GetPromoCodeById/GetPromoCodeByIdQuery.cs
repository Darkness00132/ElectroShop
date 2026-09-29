using Application.Features.PromoCodes.Dtos;
using MediatR;

namespace Application.Features.PromoCodes.Queries.GetPromoCodeById;

public sealed record GetPromoCodeByIdQuery(Guid Id) : IRequest<PromoCodeDto>;
