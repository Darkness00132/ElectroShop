using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Newsletter.Dtos;
using MediatR;

namespace Application.Features.Newsletter.Queries.GetNewsletterSubscribers;

public sealed record GetNewsletterSubscribersQuery(PaginationRequest Pagination) : IRequest<PagedResult<NewsletterSubscriberDto>>;
