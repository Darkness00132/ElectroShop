using Application.Abstractions.Repositories;
using Application.Common.Pagination;
using Application.Features.Reviews.Dtos;
using Domain.Entities.ReviewsAggregate;
using MediatR;

namespace Application.Features.Reviews.Queries.GetProductReviews;

internal class GetProductReviewsHandler : IRequestHandler<GetProductReviewsQuery, PagedResult<ReviewDto>>
{
    private readonly IRepository<Review> _reviewRepository;

    public GetProductReviewsHandler(IRepository<Review> reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<PagedResult<ReviewDto>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        return await _reviewRepository.ProjectToPagedAsync<ReviewDto>(
            request.Pagination,
            orderBy: review => review.CreatedAt,
            predicate: review => review.ProductId == request.ProductId,
            descending: true,
            cancellationToken: cancellationToken);
    }
}
