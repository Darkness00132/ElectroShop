using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Commands.SetPurchaseOrderCosts;

public sealed record SetPurchaseOrderCostsCommand(Guid PurchaseOrderId, decimal TaxAmount, decimal ShippingCost) : IRequest;
