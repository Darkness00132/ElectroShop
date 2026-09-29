using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.UpdateProduct;
using AutoMapper;
using Ecommerce.Api.Extensions;

namespace Ecommerce.Api.Contracts.Products;

public class ProductMapping : Profile
{
    public ProductMapping()
    {
        CreateMap<CreateProductRequest, CreateProductCommand>()
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Images.Select(f => f.ToFileDto()).ToList()));

        CreateMap<UpdateProductRequest, UpdateProductCommand>()
            .ForMember(d => d.NewImages, o => o.MapFrom(s =>
                s.NewImages == null
                    ? null
                    : s.NewImages.Select(f => f.ToFileDto()).ToList()));
    }
}
