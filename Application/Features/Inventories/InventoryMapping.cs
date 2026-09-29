using Application.Features.Inventories.Dtos;
using AutoMapper;
using Domain.Entities.InventoryAggregate;

namespace Application.Features.Inventories;

internal class InventoryMapping : Profile
{
    public InventoryMapping()
    {
        CreateMap<Inventory, InventoryDto>()
            .ForMember(d => d.SKU, o => o.MapFrom(s => s.Product.SKU))
            .ForMember(d => d.IsLowStock, o => o.MapFrom(s => s.QuantityOnHand <= s.ReorderLevel));
    }
}
