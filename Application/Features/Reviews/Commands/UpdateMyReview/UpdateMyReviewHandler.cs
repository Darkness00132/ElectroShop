using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Domain.Entities.ReviewsAggregate;
using MediatR;

namespace Application.Features.Reviews.Commands.UpdateMyReview;

internal class UpdateMyReviewHandler : IRequestHandler<UpdateMyReviewCommand>
{
    private readonly IRepository<Review> _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateMyReviewHandler(IRepository<Review> reviewRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateMyReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.SingleOrDefaultAsync(
            r => r.Id == request.ReviewId && r.UserId == _currentUser.UserId,
            cancellationToken)
            ?? throw new NotFoundException(nameof(Review), request.ReviewId);

        review.Update(request.Rating, request.Comment);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
