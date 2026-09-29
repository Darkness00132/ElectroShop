using Application.Features.Orders.Dtos;
using AutoMapper;
using Domain.Entities.OrdersAggregate;

namespace Application.Features.Orders.Mapping;

internal class OrderMapping : Profile
{
    public OrderMapping()
    {
        CreateMap<Order, OrderSummaryDto>()
            .ForMember(d => d.ItemsCount, o => o.MapFrom(s => s.Items.Count));

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.ShippingAddress, o => o.MapFrom(s =>
                new OrderAddressDto(
                    s.ShippingAddress.Street,
                    s.ShippingAddress.City,
                    s.ShippingAddress.Phone,
                    s.ShippingAddress.Notes)));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.LineSubtotal, o => o.MapFrom(s => s.UnitPrice * s.Quantity))
            .ForMember(d => d.LineDiscount, o => o.MapFrom(s => s.DiscountAmount * s.Quantity))
            .ForMember(d => d.LineTotal, o => o.MapFrom(s => (s.UnitPrice - s.DiscountAmount) * s.Quantity));
    }
}
