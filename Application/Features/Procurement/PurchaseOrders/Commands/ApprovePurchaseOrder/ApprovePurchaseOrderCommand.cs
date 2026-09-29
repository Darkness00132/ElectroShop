using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Commands.ApprovePurchaseOrder;

public sealed record ApprovePurchaseOrderCommand(Guid PurchaseOrderId) : IRequest;
