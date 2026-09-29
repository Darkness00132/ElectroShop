using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Procurement.GoodsReceipts.Dtos;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.GoodsReceipts.Queries.GetGoodsReceiptById;

internal class GetGoodsReceiptByIdHandler : IRequestHandler<GetGoodsReceiptByIdQuery, GoodsReceiptDto>
{
    private readonly IRepository<GoodsReceipt> _goodsReceiptRepository;

    public GetGoodsReceiptByIdHandler(IRepository<GoodsReceipt> goodsReceiptRepository)
    {
        _goodsReceiptRepository = goodsReceiptRepository;
    }

    public async Task<GoodsReceiptDto> Handle(GetGoodsReceiptByIdQuery request, CancellationToken cancellationToken)
    {
        var goodsReceipt = await _goodsReceiptRepository.ProjectToSingleOrDefaultAsync<GoodsReceiptDto>(
            gr => gr.Id == request.Id,
            cancellationToken);

        if (goodsReceipt is null)
            throw new NotFoundException(nameof(GoodsReceipt), request.Id);

        return goodsReceipt;
    }
}
