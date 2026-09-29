using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Procurement.PurchaseOrders.Dtos;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Queries.GetPurchaseOrderById;

internal class GetPurchaseOrderByIdHandler : IRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDto>
{
    private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;

    public GetPurchaseOrderByIdHandler(IRepository<PurchaseOrder> purchaseOrderRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
    }

    public async Task<PurchaseOrderDto> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.ProjectToSingleOrDefaultAsync<PurchaseOrderDto>(
            po => po.Id == request.Id,
            cancellationToken);

        if (purchaseOrder is null)
            throw new NotFoundException(nameof(PurchaseOrder), request.Id);

        return purchaseOrder;
    }
}
