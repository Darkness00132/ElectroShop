using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Commands.SubmitPurchaseOrder;

internal class SubmitPurchaseOrderHandler : IRequestHandler<SubmitPurchaseOrderCommand>
{
    private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitPurchaseOrderHandler(IRepository<PurchaseOrder> purchaseOrderRepository, IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SubmitPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(PurchaseOrder), request.PurchaseOrderId);

        purchaseOrder.SubmitForApproval();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
