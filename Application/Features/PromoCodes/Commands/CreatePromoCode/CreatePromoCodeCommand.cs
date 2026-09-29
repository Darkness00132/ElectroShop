using Application.Abstractions;
using Domain.Enums;
using MediatR;

namespace Application.Features.PromoCodes.Commands.CreatePromoCode;

public sealed record CreatePromoCodeCommand(
    string Code,
    PromoDiscountType DiscountType,
    decimal Value,
    decimal MinimumOrder,
    DateOnly StartDate,
    DateOnly EndDate,
    int? UsageLimit) : IRequest<Guid>;
