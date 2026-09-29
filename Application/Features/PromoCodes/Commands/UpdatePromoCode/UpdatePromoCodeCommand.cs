using Application.Abstractions;
using Domain.Enums;
using MediatR;

namespace Application.Features.PromoCodes.Commands.UpdatePromoCode;

public sealed record UpdatePromoCodeCommand(
    Guid Id,
    string? Code,
    PromoDiscountType? DiscountType,
    decimal? Value,
    decimal? MinimumOrder,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int? UsageLimit,
    bool? IsActive) : IRequest;
