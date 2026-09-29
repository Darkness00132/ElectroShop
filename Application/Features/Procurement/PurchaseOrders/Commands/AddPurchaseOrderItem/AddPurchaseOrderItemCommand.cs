using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Commands.AddPurchaseOrderItem;

public sealed record AddPurchaseOrderItemCommand(
    Guid PurchaseOrderId,
    Guid ProductId,
    int OrderedQuantity,
    decimal UnitCost) : IRequest;
