using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Newsletter.Commands.SubscribeToNewsletter;
using Application.Features.Newsletter.Commands.UnsubscribeFromNewsletter;
using Domain.Entities.NewsletterAggregate;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Newsletter;

public class NewsletterHandlerTests
{
    private readonly Mock<IRepository<NewsletterSubscriber>> _subscriberRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public NewsletterHandlerTests()
    {
        _subscriberRepositoryMock = new Mock<IRepository<NewsletterSubscriber>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task An_Email_Can_Be_Subscribed()
    {
        // Arrange
        SetupSubscriberMissing();

        NewsletterSubscriber? addedSubscriber = null;

        _subscriberRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<NewsletterSubscriber>(), It.IsAny<CancellationToken>()))
            .Callback<NewsletterSubscriber, CancellationToken>((subscriber, _) => addedSubscriber = subscriber);

        var handler = new SubscribeToNewsletterHandler(_subscriberRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new SubscribeToNewsletterCommand("customer@example.com"), CancellationToken.None);

        // Assert
        addedSubscriber.Should().NotBeNull();
        addedSubscriber!.Email.Should().Be("customer@example.com");
        addedSubscriber.IsSubscribed.Should().BeTrue();
    }

    [Fact]
    public async Task Subscribing_An_Already_Subscribed_Email_Is_Rejected()
    {
        // Arrange
        var subscriber = new NewsletterSubscriber("customer@example.com");
        SetupSubscriberFound(subscriber);

        var handler = new SubscribeToNewsletterHandler(_subscriberRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new SubscribeToNewsletterCommand("customer@example.com"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task An_Unsubscribed_Email_Can_Resubscribe_Without_Duplication()
    {
        // Arrange
        var subscriber = new NewsletterSubscriber("customer@example.com");
        subscriber.Unsubscribe();
        SetupSubscriberFound(subscriber);

        var handler = new SubscribeToNewsletterHandler(_subscriberRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new SubscribeToNewsletterCommand("customer@example.com"), CancellationToken.None);

        // Assert
        subscriber.IsSubscribed.Should().BeTrue();

        _subscriberRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<NewsletterSubscriber>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task An_Email_Can_Be_Unsubscribed()
    {
        // Arrange
        var subscriber = new NewsletterSubscriber("customer@example.com");
        SetupSubscriberFound(subscriber);

        var handler = new UnsubscribeFromNewsletterHandler(_subscriberRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new UnsubscribeFromNewsletterCommand("customer@example.com"), CancellationToken.None);

        // Assert
        subscriber.IsSubscribed.Should().BeFalse();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Unsubscribing_An_Unknown_Email_Is_A_No_Op()
    {
        // Arrange
        SetupSubscriberMissing();

        var handler = new UnsubscribeFromNewsletterHandler(_subscriberRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new UnsubscribeFromNewsletterCommand("unknown@example.com"), CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupSubscriberFound(NewsletterSubscriber subscriber)
    {
        _subscriberRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<NewsletterSubscriber, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriber);
    }

    private void SetupSubscriberMissing()
    {
        _subscriberRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<NewsletterSubscriber, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((NewsletterSubscriber?)null);
    }
}
