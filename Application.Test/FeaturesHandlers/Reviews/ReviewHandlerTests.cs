using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Application.Features.Reviews.Commands.CreateReview;
using Application.Features.Reviews.Commands.DeleteReview;
using Application.Features.Reviews.Commands.UpdateMyReview;
using Domain.Entities.Catalog;
using Domain.Entities.OrdersAggregate;
using Domain.Entities.ReviewsAggregate;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Reviews;

public class ReviewHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    private readonly Mock<IRepository<Review>> _reviewRepositoryMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IRepository<Order>> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;

    public ReviewHandlerTests()
    {
        _reviewRepositoryMock = new Mock<IRepository<Review>>();
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _orderRepositoryMock = new Mock<IRepository<Order>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(_userId);
    }

    [Fact]
    public async Task A_Customer_Who_Purchased_The_Product_Can_Review_It()
    {
        // Arrange
        var product = CreateProduct();
        SetupProductFound(product);
        SetupHasPurchased(true);
        SetupNoExistingReview();

        Review? addedReview = null;

        _reviewRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()))
            .Callback<Review, CancellationToken>((review, _) => addedReview = review);

        var handler = new CreateReviewHandler(
            _reviewRepositoryMock.Object,
            _productRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserMock.Object);

        // Act
        var reviewId = await handler.Handle(new CreateReviewCommand(product.Id, 5, "Great laptop."), CancellationToken.None);

        // Assert
        reviewId.Should().Be(addedReview!.Id);
        addedReview.Rating.Should().Be(5);
        addedReview.Comment.Should().Be("Great laptop.");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Customer_Who_Did_Not_Purchase_The_Product_Cannot_Review_It()
    {
        // Arrange
        var product = CreateProduct();
        SetupProductFound(product);
        SetupHasPurchased(false);

        var handler = new CreateReviewHandler(
            _reviewRepositoryMock.Object,
            _productRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserMock.Object);

        // Act
        var act = () => handler.Handle(new CreateReviewCommand(product.Id, 5, null), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task A_Product_Cannot_Be_Reviewed_Twice_By_The_Same_Customer()
    {
        // Arrange
        var product = CreateProduct();
        SetupProductFound(product);
        SetupHasPurchased(true);
        SetupExistingReview();

        var handler = new CreateReviewHandler(
            _reviewRepositoryMock.Object,
            _productRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserMock.Object);

        // Act
        var act = () => handler.Handle(new CreateReviewCommand(product.Id, 5, null), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task A_Customer_Cannot_Update_Another_Customers_Review()
    {
        // Arrange
        _reviewRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        var handler = new UpdateMyReviewHandler(_reviewRepositoryMock.Object, _unitOfWorkMock.Object, _currentUserMock.Object);

        // Act
        var act = () => handler.Handle(new UpdateMyReviewCommand(Guid.NewGuid(), 4, null), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task A_Review_Can_Be_Removed_For_Moderation()
    {
        // Arrange
        var review = new Review(_userId, Guid.NewGuid(), 3, "Average.");
        _reviewRepositoryMock
            .Setup(x => x.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var handler = new DeleteReviewHandler(_reviewRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new DeleteReviewCommand(review.Id), CancellationToken.None);

        // Assert
        _reviewRepositoryMock.Verify(
            x => x.Remove(review),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Product CreateProduct()
    {
        return new Product(
            "Gaming Laptop",
            "لابتوب ألعاب",
            "A laptop for gaming.",
            "لابتوب للألعاب.",
            "SKU-001",
            200m,
            Guid.NewGuid(),
            Guid.NewGuid());
    }

    private void SetupProductFound(Product product)
    {
        _productRepositoryMock
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private void SetupHasPurchased(bool hasPurchased)
    {
        _orderRepositoryMock
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasPurchased);
    }

    private void SetupNoExistingReview()
    {
        _reviewRepositoryMock
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupExistingReview()
    {
        _reviewRepositoryMock
            .Setup(x => x.ExistsAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }
}
