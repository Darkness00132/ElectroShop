using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Domain.Entities.Catalog;
using Domain.Entities.OrdersAggregate;
using Domain.Entities.ReviewsAggregate;
using MediatR;

namespace Application.Features.Reviews.Commands.CreateReview;

internal class CreateReviewHandler : IRequestHandler<CreateReviewCommand, Guid>
{
    private readonly IRepository<Review> _reviewRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateReviewHandler(IRepository<Review> reviewRepository, IRepository<Product> productRepository, IRepository<Order> orderRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        if (!await _productRepository.ExistsAsync(p => p.Id == request.ProductId, cancellationToken))
            throw new NotFoundException(nameof(Product), request.ProductId);

        var hasPurchased = await _orderRepository.ExistsAsync(
            o => o.UserId == userId && o.Items.Any(item => item.ProductId == request.ProductId),
            cancellationToken);

        if (!hasPurchased)
            throw new ForbiddenException("Only customers who purchased the product can review it.");

        if (await _reviewRepository.ExistsAsync(
            r => r.UserId == userId && r.ProductId == request.ProductId,
            cancellationToken)) {
            throw new ConflictException("You have already reviewed this product.");
        }

        var review = new Review(userId, request.ProductId, request.Rating, request.Comment);

        await _reviewRepository.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return review.Id;
    }
}
