using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.Catalog;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.GoodsReceipts.Commands.AddGoodsReceiptItem;

internal class AddGoodsReceiptItemHandler : IRequestHandler<AddGoodsReceiptItemCommand>
{
    private readonly IRepository<GoodsReceipt> _goodsReceiptRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddGoodsReceiptItemHandler(IRepository<GoodsReceipt> goodsReceiptRepository, IRepository<Product> productRepository, IUnitOfWork unitOfWork)
    {
        _goodsReceiptRepository = goodsReceiptRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddGoodsReceiptItemCommand request, CancellationToken cancellationToken)
    {
        var goodsReceipt = await _goodsReceiptRepository.GetByIdAsync(request.GoodsReceiptId, cancellationToken)
            ?? throw new NotFoundException(nameof(GoodsReceipt), request.GoodsReceiptId);

        if (!await _productRepository.ExistsAsync(p => p.Id == request.ProductId, cancellationToken))
            throw new NotFoundException(nameof(Product), request.ProductId);

        goodsReceipt.AddItem(request.ProductId, request.Quantity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
