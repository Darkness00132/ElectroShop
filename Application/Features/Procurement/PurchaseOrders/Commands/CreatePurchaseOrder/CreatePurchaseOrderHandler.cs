using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Commands.CreatePurchaseOrder;

internal class CreatePurchaseOrderHandler : IRequestHandler<CreatePurchaseOrderCommand, Guid>
{
    private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePurchaseOrderHandler(IRepository<PurchaseOrder> purchaseOrderRepository, IRepository<Supplier> supplierRepository, IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        if (!await _supplierRepository.ExistsAsync(s => s.Id == request.SupplierId, cancellationToken))
            throw new NotFoundException(nameof(Supplier), request.SupplierId);

        var normalizedNumber = request.Number.Trim();

        if (await _purchaseOrderRepository.ExistsAsync(po => po.Number == normalizedNumber, cancellationToken))
            throw new ConflictException("A purchase order with the same number already exists.");

        var purchaseOrder = new PurchaseOrder(
            request.Number,
            request.SupplierId,
            request.OrderDate,
            request.ExpectedDeliveryDate,
            request.Notes);

        await _purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return purchaseOrder.Id;
    }
}
