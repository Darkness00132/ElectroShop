using Domain.Enums;

namespace Ecommerce.Api.Contracts.PromoCodes;

/// <summary>
/// Represents the request body for creating a promo code.
/// </summary>
/// <param name="Code">The promo code; stored uppercase and must be unique.</param>
/// <param name="DiscountType">Whether the promo is a percentage or a fixed amount.</param>
/// <param name="Value">The discount value; 1-100 for percentages, otherwise the absolute amount.</param>
/// <param name="MinimumOrder">The minimum order total required to use the promo code.</param>
/// <param name="StartDate">The first day the promo code is valid.</param>
/// <param name="EndDate">The last day the promo code is valid.</param>
/// <param name="UsageLimit">The optional maximum number of times the promo code can be used.</param>
public sealed record CreatePromoCodeRequest(
    string Code,
    PromoDiscountType DiscountType,
    decimal Value,
    decimal MinimumOrder,
    DateOnly StartDate,
    DateOnly EndDate,
    int? UsageLimit);

/// <summary>
/// Represents the request body for partially updating a promo code.
/// Omitted fields keep their current values.
/// </summary>
/// <param name="Code">The new promo code, or null to keep the current one.</param>
/// <param name="DiscountType">The new discount type, or null to keep the current one.</param>
/// <param name="Value">The new discount value, or null to keep the current one.</param>
/// <param name="MinimumOrder">The new minimum order total, or null to keep the current one.</param>
/// <param name="StartDate">The new start date, or null to keep the current one.</param>
/// <param name="EndDate">The new end date, or null to keep the current one.</param>
/// <param name="UsageLimit">The new usage limit, or null to keep the current one.</param>
/// <param name="IsActive">Whether the promo code is usable, or null to keep the current state.</param>
public sealed record UpdatePromoCodeRequest(
    string? Code,
    PromoDiscountType? DiscountType,
    decimal? Value,
    decimal? MinimumOrder,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int? UsageLimit,
    bool? IsActive);
