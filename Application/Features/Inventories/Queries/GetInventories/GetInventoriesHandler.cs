using Application.Abstractions.Repositories;
using Application.Common.Pagination;
using Application.Features.Inventories.Dtos;
using Domain.Entities.InventoryAggregate;
using MediatR;

namespace Application.Features.Inventories.Queries.GetInventories;

internal class GetInventoriesHandler : IRequestHandler<GetInventoriesQuery, PagedResult<InventoryDto>>
{
    private readonly IRepository<Inventory> _inventoryRepository;

    public GetInventoriesHandler(IRepository<Inventory> inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<PagedResult<InventoryDto>> Handle(GetInventoriesQuery request, CancellationToken cancellationToken)
    {
        return await _inventoryRepository.ProjectToPagedAsync<InventoryDto>(
            request.Pagination,
            orderBy: inventory => inventory.QuantityOnHand,
            cancellationToken: cancellationToken);
    }
}
