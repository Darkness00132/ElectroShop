using Application.Features.Products.Dtos;
using AutoMapper;
using Domain.Entities.Catalog;

namespace Application.Features.Products;

internal class ProductMapping : Profile
{
    public ProductMapping()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        CreateMap<Product, ProductInList>()
            .ForMember(d => d.BrandNameAr, o => o.MapFrom(s => s.Brand.NameAr))
            .ForMember(d => d.BrandNameEn, o => o.MapFrom(s => s.Brand.NameEn))
            .ForMember(d => d.CategoryNameAr, o => o.MapFrom(s => s.Category.NameAr))
            .ForMember(d => d.CategoryNameEn, o => o.MapFrom(s => s.Category.NameEn))
            .ForMember(d => d.Discount,
                o => o.MapFrom(s =>
                    s.Discount != null &&
                    s.Discount.IsVisible &&
                    s.Discount.ValidityPeriod.StartDate <= today &&
                    s.Discount.ValidityPeriod.EndDate >= today
                        ? s.Discount
                        : null));

        CreateMap<Product, DetailedProduct>()
                .ForMember(d => d.BrandNameAr, o => o.MapFrom(s => s.Brand.NameAr))
                .ForMember(d => d.BrandNameEn, o => o.MapFrom(s => s.Brand.NameEn))
                .ForMember(d => d.CategoryNameAr, o => o.MapFrom(s => s.Category.NameAr))
                .ForMember(d => d.CategoryNameEn, o => o.MapFrom(s => s.Category.NameEn))
                .ForMember(d => d.Quantity,
                    o => o.MapFrom(s => s.Inventory.QuantityOnHand))
                .ForMember(d => d.Discount,
                    o => o.MapFrom(s =>
                        s.Discount != null &&
                        s.Discount.IsVisible &&
                        s.Discount.ValidityPeriod.StartDate <= today &&
                        s.Discount.ValidityPeriod.EndDate >= today
                            ? s.Discount
                            : null));

        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<ProductAttribute, ProductAttributeDto>();
        CreateMap<Discount, DiscountInProduct>()
            .ForMember(d => d.StartDate,
            o => o.MapFrom(s => s.ValidityPeriod.StartDate))
            .ForMember(d => d.EndDate, o => o.MapFrom(s => s.ValidityPeriod.EndDate));
    }
}
