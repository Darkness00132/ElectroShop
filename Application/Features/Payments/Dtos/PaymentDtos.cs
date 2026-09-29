using Domain.Enums;

namespace Application.Features.Payments.Dtos;

public sealed record PaymentDto(
    Guid Id,
    Guid OrderId,
    PaymentStatus Status,
    decimal Amount,
    DateTime CreatedAt,
    DateTime? PaidAt,
    DateTime? RefundedAt,
    IReadOnlyList<PaymentAttemptDto> Attempts);

public sealed record PaymentAttemptDto(
    Guid Id,
    PaymentMethod Method,
    PaymentAttemptStatus Status,
    decimal Amount,
    string? TransactionId,
    DateTime CreatedAt,
    DateTime? CompletedAt);
