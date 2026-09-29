using Application.Abstractions.Repositories;
using Application.Common.Pagination;
using Application.Features.Procurement.GoodsReceipts.Dtos;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.GoodsReceipts.Queries.GetGoodsReceipts;

internal class GetGoodsReceiptsHandler : IRequestHandler<GetGoodsReceiptsQuery, PagedResult<GoodsReceiptDto>>
{
    private readonly IRepository<GoodsReceipt> _goodsReceiptRepository;

    public GetGoodsReceiptsHandler(IRepository<GoodsReceipt> goodsReceiptRepository)
    {
        _goodsReceiptRepository = goodsReceiptRepository;
    }

    public async Task<PagedResult<GoodsReceiptDto>> Handle(GetGoodsReceiptsQuery request, CancellationToken cancellationToken)
    {
        return await _goodsReceiptRepository.ProjectToPagedAsync<GoodsReceiptDto>(
            request.Pagination,
            orderBy: goodsReceipt => goodsReceipt.CreatedAt,
            descending: true,
            cancellationToken: cancellationToken);
    }
}
