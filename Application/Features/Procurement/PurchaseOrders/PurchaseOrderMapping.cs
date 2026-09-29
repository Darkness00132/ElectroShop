using Application.Features.Procurement.PurchaseOrders.Dtos;
using AutoMapper;
using Domain.Entities.ProcurementAggregate;

namespace Application.Features.Procurement.PurchaseOrders;

internal class PurchaseOrderMapping : Profile
{
    public PurchaseOrderMapping()
    {
        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.Name));

        CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
            .ForMember(d => d.ProductNameEn, o => o.MapFrom(s => s.Product.NameEn));
    }
}
