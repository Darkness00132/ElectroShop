using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.NewsletterAggregate;
using FluentValidation;
using MediatR;

namespace Application.Features.Newsletter.Commands.SubscribeToNewsletter;

internal class SubscribeToNewsletterValidator : AbstractValidator<SubscribeToNewsletterCommand>
{
    public SubscribeToNewsletterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}

internal class SubscribeToNewsletterHandler : IRequestHandler<SubscribeToNewsletterCommand>
{
    private readonly IRepository<NewsletterSubscriber> _subscriberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubscribeToNewsletterHandler(IRepository<NewsletterSubscriber> subscriberRepository, IUnitOfWork unitOfWork)
    {
        _subscriberRepository = subscriberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SubscribeToNewsletterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        var existingSubscriber = await _subscriberRepository.SingleOrDefaultAsync(
            s => s.Email == email,
            cancellationToken);

        if (existingSubscriber is not null) {
            if (existingSubscriber.IsSubscribed)
                throw new ConflictException("This email is already subscribed to the newsletter.");

            existingSubscriber.Subscribe();
        }
        else {
            await _subscriberRepository.AddAsync(new NewsletterSubscriber(email), cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
