using MediatR;

namespace Application.Features.Newsletter.Commands.UnsubscribeFromNewsletter;

public sealed record UnsubscribeFromNewsletterCommand(string Email) : IRequest;
