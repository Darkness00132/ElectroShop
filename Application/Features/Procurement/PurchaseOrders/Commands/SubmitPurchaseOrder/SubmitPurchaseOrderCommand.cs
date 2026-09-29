using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Commands.SubmitPurchaseOrder;

public sealed record SubmitPurchaseOrderCommand(Guid PurchaseOrderId) : IRequest;
