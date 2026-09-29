using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Commands.SetPurchaseOrderCosts;

internal class SetPurchaseOrderCostsHandler : IRequestHandler<SetPurchaseOrderCostsCommand>
{
    private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetPurchaseOrderCostsHandler(IRepository<PurchaseOrder> purchaseOrderRepository, IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetPurchaseOrderCostsCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(PurchaseOrder), request.PurchaseOrderId);

        purchaseOrder.SetCosts(request.TaxAmount, request.ShippingCost);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
