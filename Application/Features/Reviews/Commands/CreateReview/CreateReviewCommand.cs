using MediatR;

namespace Application.Features.Reviews.Commands.CreateReview;

public sealed record CreateReviewCommand(Guid ProductId, int Rating, string? Comment) : IRequest<Guid>;
