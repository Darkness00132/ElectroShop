using MediatR;

namespace Application.Features.Reviews.Commands.UpdateMyReview;

public sealed record UpdateMyReviewCommand(Guid ReviewId, int Rating, string? Comment) : IRequest;
