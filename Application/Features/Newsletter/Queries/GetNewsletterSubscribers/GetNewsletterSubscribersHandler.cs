using Application.Abstractions.Repositories;
using Application.Common.Pagination;
using Application.Features.Newsletter.Dtos;
using Domain.Entities.NewsletterAggregate;
using MediatR;

namespace Application.Features.Newsletter.Queries.GetNewsletterSubscribers;

internal class GetNewsletterSubscribersHandler : IRequestHandler<GetNewsletterSubscribersQuery, PagedResult<NewsletterSubscriberDto>>
{
    private readonly IRepository<NewsletterSubscriber> _subscriberRepository;

    public GetNewsletterSubscribersHandler(IRepository<NewsletterSubscriber> subscriberRepository)
    {
        _subscriberRepository = subscriberRepository;
    }

    public async Task<PagedResult<NewsletterSubscriberDto>> Handle(GetNewsletterSubscribersQuery request, CancellationToken cancellationToken)
    {
        return await _subscriberRepository.ProjectToPagedAsync<NewsletterSubscriberDto>(
            request.Pagination,
            orderBy: subscriber => subscriber.CreatedAt,
            descending: true,
            cancellationToken: cancellationToken);
    }
}
