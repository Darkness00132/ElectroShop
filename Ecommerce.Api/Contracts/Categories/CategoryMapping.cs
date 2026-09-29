using Application.Features.Categories.Commands.CreateCategory;
using Application.Features.Categories.Commands.UpdateCategory;
using AutoMapper;
using Ecommerce.Api.Extensions;

namespace Ecommerce.Api.Contracts.Categories;

public class CategoryMapping : Profile
{
    public CategoryMapping()
    {
        CreateMap<CreateCategoryRequest, CreateCategoryCommand>()
            .ForMember(d => d.Image, o => o.MapFrom(s => s.Image.ToFileDto()));

        CreateMap<UpdateCategoryRequest, UpdateCategoryCommand>()
            .ForMember(
                d => d.NewImage,
                o => o.MapFrom((src) =>
                    src.NewImage == null ? null : src.NewImage.ToFileDto()));
    }
}
