using Application.Features.Procurement.PurchaseOrders.Dtos;
using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Queries.GetPurchaseOrderById;

public sealed record GetPurchaseOrderByIdQuery(Guid Id) : IRequest<PurchaseOrderDto>;
