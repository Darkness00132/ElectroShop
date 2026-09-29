namespace Ecommerce.Api.Contracts.Newsletter;

/// <summary>
/// Represents the request body for subscribing to the newsletter.
/// </summary>
/// <param name="Email">The email address to subscribe.</param>
public sealed record SubscribeRequest(string Email);

/// <summary>
/// Represents the request body for unsubscribing from the newsletter.
/// </summary>
/// <param name="Email">The email address to unsubscribe.</param>
public sealed record UnsubscribeRequest(string Email);
