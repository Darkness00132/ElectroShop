using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.InventoryAggregate;
using MediatR;

namespace Application.Features.Inventories.Commands.ChangeReorderLevel;

internal class ChangeReorderLevelHandler : IRequestHandler<ChangeReorderLevelCommand>
{
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeReorderLevelHandler(IRepository<Inventory> inventoryRepository, IUnitOfWork unitOfWork)
    {
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ChangeReorderLevelCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.SingleOrDefaultAsync(
            i => i.ProductId == request.ProductId,
            cancellationToken)
            ?? throw new NotFoundException(nameof(Inventory), request.ProductId);

        inventory.ChangeReorderLevel(request.ReorderLevel);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
