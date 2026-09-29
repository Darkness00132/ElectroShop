using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.Catalog;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.PurchaseOrders.Commands.AddPurchaseOrderItem;

internal class AddPurchaseOrderItemHandler : IRequestHandler<AddPurchaseOrderItemCommand>
{
    private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddPurchaseOrderItemHandler(IRepository<PurchaseOrder> purchaseOrderRepository, IRepository<Product> productRepository, IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddPurchaseOrderItemCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(PurchaseOrder), request.PurchaseOrderId);

        if (!await _productRepository.ExistsAsync(p => p.Id == request.ProductId, cancellationToken))
            throw new NotFoundException(nameof(Product), request.ProductId);

        purchaseOrder.AddItem(request.ProductId, request.OrderedQuantity, request.UnitCost);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
