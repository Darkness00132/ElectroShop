using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.GoodsReceipts.Commands.CreateGoodsReceipt;

internal class CreateGoodsReceiptHandler : IRequestHandler<CreateGoodsReceiptCommand, Guid>
{
    private readonly IRepository<GoodsReceipt> _goodsReceiptRepository;
    private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGoodsReceiptHandler(IRepository<GoodsReceipt> goodsReceiptRepository, IRepository<PurchaseOrder> purchaseOrderRepository, IUnitOfWork unitOfWork)
    {
        _goodsReceiptRepository = goodsReceiptRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        if (!await _purchaseOrderRepository.ExistsAsync(po => po.Id == request.PurchaseOrderId, cancellationToken))
            throw new NotFoundException(nameof(PurchaseOrder), request.PurchaseOrderId);

        var normalizedNumber = request.Number.Trim();

        if (await _goodsReceiptRepository.ExistsAsync(gr => gr.Number == normalizedNumber, cancellationToken))
            throw new ConflictException("A goods receipt with the same number already exists.");

        var goodsReceipt = new GoodsReceipt(
            request.Number,
            request.PurchaseOrderId,
            request.ReceivedAt,
            request.DeliveryReference,
            request.Notes);

        await _goodsReceiptRepository.AddAsync(goodsReceipt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return goodsReceipt.Id;
    }
}
