using MediatR;

namespace Application.Features.Carts.Commands.AddCartItem;

public sealed record AddCartItemCommand(Guid ProductId, int Quantity) : IRequest;
