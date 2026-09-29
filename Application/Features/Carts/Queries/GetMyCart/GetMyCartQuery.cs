using Application.Features.Carts.Dtos;
using MediatR;

namespace Application.Features.Carts.Queries.GetMyCart;

public sealed record GetMyCartQuery : IRequest<CartDto>;
