using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.ReviewsAggregate;
using MediatR;

namespace Application.Features.Reviews.Commands.DeleteReview;

internal class DeleteReviewHandler : IRequestHandler<DeleteReviewCommand>
{
    private readonly IRepository<Review> _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReviewHandler(IRepository<Review> reviewRepository, IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken)
            ?? throw new NotFoundException(nameof(Review), request.ReviewId);

        _reviewRepository.Remove(review);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
