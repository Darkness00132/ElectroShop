using MediatR;

namespace Application.Features.Carts.Commands.UpdateCartItem;

public sealed record UpdateCartItemCommand(Guid ProductId, int Quantity) : IRequest;
