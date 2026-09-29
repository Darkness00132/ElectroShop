using Application.Abstractions.Repositories;
using Application.Common.Pagination;
using Application.Features.Procurement.PurchaseOrders.Dtos;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Queries.GetPurchaseOrders;

internal class GetPurchaseOrdersHandler : IRequestHandler<GetPurchaseOrdersQuery, PagedResult<PurchaseOrderDto>>
{
    private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;

    public GetPurchaseOrdersHandler(IRepository<PurchaseOrder> purchaseOrderRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
    }

    public async Task<PagedResult<PurchaseOrderDto>> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _purchaseOrderRepository.ProjectToPagedAsync<PurchaseOrderDto>(
            request.Pagination,
            orderBy: purchaseOrder => purchaseOrder.CreatedAt,
            descending: true,
            cancellationToken: cancellationToken);
    }
}
