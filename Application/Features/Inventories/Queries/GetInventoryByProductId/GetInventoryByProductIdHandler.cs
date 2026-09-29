using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Inventories.Dtos;
using Domain.Entities.InventoryAggregate;
using MediatR;

namespace Application.Features.Inventories.Queries.GetInventoryByProductId;

internal class GetInventoryByProductIdHandler : IRequestHandler<GetInventoryByProductIdQuery, InventoryDto>
{
    private readonly IRepository<Inventory> _inventoryRepository;

    public GetInventoryByProductIdHandler(IRepository<Inventory> inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<InventoryDto> Handle(GetInventoryByProductIdQuery request, CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.ProjectToSingleOrDefaultAsync<InventoryDto>(
            i => i.ProductId == request.ProductId,
            cancellationToken);

        if (inventory is null)
            throw new NotFoundException(nameof(Inventory), request.ProductId);

        return inventory;
    }
}
