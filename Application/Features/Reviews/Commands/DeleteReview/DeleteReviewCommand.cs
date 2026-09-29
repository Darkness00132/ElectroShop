using MediatR;

namespace Application.Features.Reviews.Commands.DeleteReview;

public sealed record DeleteReviewCommand(Guid ReviewId) : IRequest;
