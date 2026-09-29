using Domain.Enums;

namespace Ecommerce.Api.Contracts.Discount;

/// <summary>
/// Represents the request body for partially updating an existing discount.
/// Omitted fields keep their current values.
/// </summary>
/// <param name="Name">The new discount name, or null to keep the current one.</param>
/// <param name="DiscountType">The new discount type, or null to keep the current one.</param>
/// <param name="Value">
/// The new discount value, or null to keep the current one.
/// For percentage discounts the value is 1-100; for fixed-amount discounts it is the absolute amount.
/// </param>
/// <param name="StartDate">The new start date of the validity period, or null to keep the current one.</param>
/// <param name="EndDate">The new end date of the validity period, or null to keep the current one.</param>
/// <param name="IsActive">Whether the discount is visible and applicable, or null to keep the current state.</param>
public sealed record UpdateDiscountRequest(
    string? Name,
    DiscountType? DiscountType,
    decimal? Value,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool? IsActive);
