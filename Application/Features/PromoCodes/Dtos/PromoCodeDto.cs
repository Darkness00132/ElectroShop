using Domain.Enums;

namespace Application.Features.PromoCodes.Dtos;

public sealed record PromoCodeDto(
    Guid Id,
    string Code,
    PromoDiscountType DiscountType,
    decimal Value,
    decimal MinimumOrder,
    DateOnly StartDate,
    DateOnly EndDate,
    int? UsageLimit,
    int UsedCount,
    bool IsActive);
