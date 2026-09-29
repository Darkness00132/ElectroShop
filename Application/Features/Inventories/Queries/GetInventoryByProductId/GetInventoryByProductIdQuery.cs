using Application.Features.Inventories.Dtos;
using MediatR;

namespace Application.Features.Inventories.Queries.GetInventoryByProductId;

public sealed record GetInventoryByProductIdQuery(Guid ProductId) : IRequest<InventoryDto>;
