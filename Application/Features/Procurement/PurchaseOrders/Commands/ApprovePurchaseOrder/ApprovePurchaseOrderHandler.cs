using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Commands.ApprovePurchaseOrder;

internal class ApprovePurchaseOrderHandler : IRequestHandler<ApprovePurchaseOrderCommand>
{
    private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApprovePurchaseOrderHandler(IRepository<PurchaseOrder> purchaseOrderRepository, IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(PurchaseOrder), request.PurchaseOrderId);

        purchaseOrder.Approve();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
