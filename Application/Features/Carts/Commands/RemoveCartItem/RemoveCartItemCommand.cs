using MediatR;

namespace Application.Features.Carts.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid ProductId) : IRequest;
