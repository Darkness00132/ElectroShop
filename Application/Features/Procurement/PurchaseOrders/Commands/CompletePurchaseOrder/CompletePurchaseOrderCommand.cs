using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Commands.CompletePurchaseOrder;

public sealed record CompletePurchaseOrderCommand(Guid PurchaseOrderId) : IRequest;
