using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Commands.CancelPurchaseOrder;

public sealed record CancelPurchaseOrderCommand(Guid PurchaseOrderId) : IRequest;
