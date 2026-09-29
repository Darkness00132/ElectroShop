using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Reviews.Dtos;
using MediatR;

namespace Application.Features.Reviews.Queries.GetProductReviews;

public sealed record GetProductReviewsQuery(Guid ProductId, PaginationRequest Pagination) : IRequest<PagedResult<ReviewDto>>;
