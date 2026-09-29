using Application.Features.Reviews.Dtos;
using AutoMapper;
using Domain.Entities.ReviewsAggregate;

namespace Application.Features.Reviews;

internal class ReviewMapping : Profile
{
    public ReviewMapping()
    {
        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s =>
                s.User.FullName.FirstName + " " + s.User.FullName.LastName));
    }
}
