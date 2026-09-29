namespace Ecommerce.Api.Contracts.Reviews;

/// <summary>
/// Represents the request body for reviewing a product.
/// </summary>
/// <param name="Rating">The rating from 1 to 5.</param>
/// <param name="Comment">The optional comment; at most 1000 characters.</param>
public sealed record CreateReviewRequest(int Rating, string? Comment);

/// <summary>
/// Represents the request body for updating a review.
/// </summary>
/// <param name="Rating">The new rating from 1 to 5.</param>
/// <param name="Comment">The optional comment; at most 1000 characters.</param>
public sealed record UpdateReviewRequest(int Rating, string? Comment);
