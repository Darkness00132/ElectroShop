using Application.Common.Checkout;
using MediatR;

namespace Application.Features.Carts.Queries.GetCheckoutSummary;

public sealed record GetCheckoutSummaryQuery : IRequest<CheckoutSummaryDto>;

public sealed record CheckoutSummaryDto(
    IReadOnlyList<CheckoutLine> Items,
    decimal Subtotal,
    decimal ItemsDiscountAmount,
    decimal ShippingFee,
    decimal Total);
