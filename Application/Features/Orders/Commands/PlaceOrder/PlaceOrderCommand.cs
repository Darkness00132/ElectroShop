using MediatR;

namespace Application.Features.Orders.Commands.PlaceOrder;

public sealed record PlaceOrderCommand(
    string Street,
    string City,
    string Phone,
    string? Notes,
    string? PromoCode) : IRequest<Guid>;
