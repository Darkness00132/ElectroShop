using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Commands.CancelPurchaseOrder;

internal class CancelPurchaseOrderHandler : IRequestHandler<CancelPurchaseOrderCommand>
{
    private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelPurchaseOrderHandler(IRepository<PurchaseOrder> purchaseOrderRepository, IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(PurchaseOrder), request.PurchaseOrderId);

        purchaseOrder.Cancel();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
