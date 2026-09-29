using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.GoodsReceipts.Commands.CancelGoodsReceipt;

internal class CancelGoodsReceiptHandler : IRequestHandler<CancelGoodsReceiptCommand>
{
    private readonly IRepository<GoodsReceipt> _goodsReceiptRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelGoodsReceiptHandler(IRepository<GoodsReceipt> goodsReceiptRepository, IUnitOfWork unitOfWork)
    {
        _goodsReceiptRepository = goodsReceiptRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        var goodsReceipt = await _goodsReceiptRepository.GetByIdAsync(request.GoodsReceiptId, cancellationToken)
            ?? throw new NotFoundException(nameof(GoodsReceipt), request.GoodsReceiptId);

        goodsReceipt.Cancel();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
