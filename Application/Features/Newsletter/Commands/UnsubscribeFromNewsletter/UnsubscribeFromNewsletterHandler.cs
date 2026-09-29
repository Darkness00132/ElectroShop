using Application.Abstractions.Repositories;
using Domain.Entities.NewsletterAggregate;
using MediatR;

namespace Application.Features.Newsletter.Commands.UnsubscribeFromNewsletter;

internal class UnsubscribeFromNewsletterHandler : IRequestHandler<UnsubscribeFromNewsletterCommand>
{
    private readonly IRepository<NewsletterSubscriber> _subscriberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnsubscribeFromNewsletterHandler(IRepository<NewsletterSubscriber> subscriberRepository, IUnitOfWork unitOfWork)
    {
        _subscriberRepository = subscriberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UnsubscribeFromNewsletterCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await _subscriberRepository.SingleOrDefaultAsync(
            s => s.Email == request.Email.Trim(),
            cancellationToken);

        if (subscriber is null || !subscriber.IsSubscribed)
            return;

        subscriber.Unsubscribe();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
