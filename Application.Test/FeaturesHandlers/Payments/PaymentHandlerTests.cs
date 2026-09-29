using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Payments.Commands.CreateOrderPayment;
using Application.Features.Payments.Commands.MarkPaymentPaid;
using Application.Features.Payments.Commands.RefundPayment;
using Domain.Entities.OrdersAggregate;
using Domain.Entities.PaymentsAggregate;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Payments;

public class PaymentHandlerTests
{
    private readonly Mock<IRepository<Payment>> _paymentRepositoryMock;
    private readonly Mock<IRepository<Domain.Entities.OrdersAggregate.Order>> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public PaymentHandlerTests()
    {
        _paymentRepositoryMock = new Mock<IRepository<Payment>>();
        _orderRepositoryMock = new Mock<IRepository<Domain.Entities.OrdersAggregate.Order>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task A_Cash_On_Delivery_Payment_Can_Be_Recorded_For_An_Order()
    {
        // Arrange
        var order = CreateDeliveredOrder();
        SetupOrderFound(order);
        SetupPaymentMissing(order.Id);

        Payment? addedPayment = null;

        _paymentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((payment, _) => addedPayment = payment);

        var handler = new CreateOrderPaymentHandler(_paymentRepositoryMock.Object, _orderRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var paymentId = await handler.Handle(new CreateOrderPaymentCommand(order.Id), CancellationToken.None);

        // Assert
        paymentId.Should().Be(addedPayment!.Id);
        addedPayment.OrderId.Should().Be(order.Id);
        addedPayment.Amount.Should().Be(order.Total);
        addedPayment.Status.Should().Be(PaymentStatus.Pending);
        addedPayment.Attempts.Should().ContainSingle(a => a.Method == PaymentMethod.CashOnDelivery && a.Amount == order.Total);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Payment_Cannot_Be_Recorded_Twice_For_The_Same_Order()
    {
        // Arrange
        var order = CreateDeliveredOrder();
        SetupOrderFound(order);
        SetupPaymentExists(order.Id);

        var handler = new CreateOrderPaymentHandler(_paymentRepositoryMock.Object, _orderRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new CreateOrderPaymentCommand(order.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task A_Payment_Cannot_Be_Recorded_When_The_Order_Does_Not_Exist()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.OrdersAggregate.Order?)null);

        var handler = new CreateOrderPaymentHandler(_paymentRepositoryMock.Object, _orderRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new CreateOrderPaymentCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Marking_A_Payment_As_Paid_Is_Idempotent()
    {
        // Arrange
        var payment = CreatePayment(Guid.NewGuid());
        payment.MarkAsPaid();
        var paidAt = payment.PaidAt;

        SetupPaymentFound(payment);

        var handler = new MarkPaymentPaidHandler(_paymentRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new MarkPaymentPaidCommand(payment.Id), CancellationToken.None);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Paid);
        payment.PaidAt.Should().Be(paidAt);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Paid_Payment_Can_Be_Refunded_And_The_Order_Becomes_Refunded()
    {
        // Arrange
        var order = CreateDeliveredOrder();
        var payment = CreatePayment(order.Id);
        payment.MarkAsPaid();

        SetupPaymentFound(payment);
        SetupOrderFound(order);

        var handler = new RefundPaymentHandler(_paymentRepositoryMock.Object, _orderRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new RefundPaymentCommand(payment.Id), CancellationToken.None);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.RefundedAt.Should().NotBeNull();
        order.Status.Should().Be(OrderStatus.Refunded);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task An_Unpaid_Payment_Cannot_Be_Refunded()
    {
        // Arrange
        var order = CreateDeliveredOrder();
        var payment = CreatePayment(order.Id);

        SetupPaymentFound(payment);
        SetupOrderFound(order);

        var handler = new RefundPaymentHandler(_paymentRepositoryMock.Object, _orderRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new RefundPaymentCommand(payment.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }

    private static Domain.Entities.OrdersAggregate.Order CreateDeliveredOrder()
    {
        var order = new Domain.Entities.OrdersAggregate.Order(
            Guid.NewGuid(),
            new Address("Main Street", "Cairo", "01000000000"));

        order.AddItem(Guid.NewGuid(), "Gaming Laptop", "لابتوب ألعاب", "SKU-001", 2, 200m);
        order.Confirm();
        order.StartProcessing();
        order.Ship();
        order.Deliver();

        return order;
    }

    private static Payment CreatePayment(Guid orderId)
    {
        var payment = new Payment(orderId, 400m);
        payment.AddAttempt(PaymentMethod.CashOnDelivery, 400m);

        return payment;
    }

    private void SetupOrderFound(Domain.Entities.OrdersAggregate.Order order)
    {
        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
    }

    private void SetupPaymentFound(Payment payment)
    {
        _paymentRepositoryMock
            .Setup(x => x.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
    }

    private void SetupPaymentMissing(Guid orderId)
    {
        _paymentRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupPaymentExists(Guid orderId)
    {
        _paymentRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }
}
