namespace Application.Features.Reviews.Dtos;

public sealed record ReviewDto(
    Guid Id,
    Guid ProductId,
    string UserName,
    int Rating,
    string? Comment,
    DateTime CreatedAt);
