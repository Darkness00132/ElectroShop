using Application.Features.Procurement.GoodsReceipts.Dtos;
using AutoMapper;
using Domain.Entities.ProcurementAggregate;

namespace Application.Features.Procurement.GoodsReceipts;

internal class GoodsReceiptMapping : Profile
{
    public GoodsReceiptMapping()
    {
        CreateMap<GoodsReceipt, GoodsReceiptDto>();

        CreateMap<GoodsReceiptItem, GoodsReceiptItemDto>()
            .ForMember(d => d.ProductNameEn, o => o.MapFrom(s => s.Product.NameEn));
    }
}
