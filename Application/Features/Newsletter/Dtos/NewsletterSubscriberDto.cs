namespace Application.Features.Newsletter.Dtos;

public sealed record NewsletterSubscriberDto(
    Guid Id,
    string Email,
    bool IsSubscribed,
    DateTime CreatedAt);
