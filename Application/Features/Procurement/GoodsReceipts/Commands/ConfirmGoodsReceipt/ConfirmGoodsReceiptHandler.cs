using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.GoodsReceipts.Commands.ConfirmGoodsReceipt;

internal class ConfirmGoodsReceiptHandler : IRequestHandler<ConfirmGoodsReceiptCommand>
{
    private readonly IRepository<GoodsReceipt> _goodsReceiptRepository;
    private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmGoodsReceiptHandler(IRepository<GoodsReceipt> goodsReceiptRepository, IRepository<PurchaseOrder> purchaseOrderRepository, IRepository<Inventory> inventoryRepository, IUnitOfWork unitOfWork)
    {
        _goodsReceiptRepository = goodsReceiptRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ConfirmGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        var goodsReceipt = await _goodsReceiptRepository.SingleOrDefaultAsync(
            gr => gr.Id == request.GoodsReceiptId,
            cancellationToken,
            gr => gr.Items)
            ?? throw new NotFoundException(nameof(GoodsReceipt), request.GoodsReceiptId);

        var purchaseOrder = await _purchaseOrderRepository.SingleOrDefaultAsync(
            po => po.Id == goodsReceipt.PurchaseOrderId,
            cancellationToken,
            po => po.Items)
            ?? throw new NotFoundException(nameof(PurchaseOrder), goodsReceipt.PurchaseOrderId);

        foreach (var item in goodsReceipt.Items) {
            var purchaseOrderItem = purchaseOrder.Items.FirstOrDefault(poItem => poItem.ProductId == item.ProductId)
                ?? throw new ConflictException($"The product '{item.ProductId}' does not belong to the purchase order.");

            purchaseOrderItem.Receive(item.Quantity);
        }

        goodsReceipt.Confirm();

        var productIds = goodsReceipt.Items.Select(item => item.ProductId).ToList();

        var inventories = await _inventoryRepository.ListAsync(
            i => productIds.Contains(i.ProductId),
            cancellationToken);

        foreach (var item in goodsReceipt.Items) {
            inventories
                .Single(inventory => inventory.ProductId == item.ProductId)
                .IncreaseStock(item.Quantity, goodsReceiptId: goodsReceipt.Id);
        }

        if (purchaseOrder.Items.All(poItem => poItem.ReceivedQuantity >= poItem.OrderedQuantity))
            purchaseOrder.Complete();
        else
            purchaseOrder.MarkAsPartiallyReceived();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
