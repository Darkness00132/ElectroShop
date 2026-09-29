using MediatR;

namespace Application.Features.Newsletter.Commands.SubscribeToNewsletter;

public sealed record SubscribeToNewsletterCommand(string Email) : IRequest;
