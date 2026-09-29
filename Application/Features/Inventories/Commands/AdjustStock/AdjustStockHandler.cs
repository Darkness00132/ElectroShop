using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.InventoryAggregate;
using MediatR;

namespace Application.Features.Inventories.Commands.AdjustStock;

internal class AdjustStockHandler : IRequestHandler<AdjustStockCommand>
{
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdjustStockHandler(IRepository<Inventory> inventoryRepository, IUnitOfWork unitOfWork)
    {
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.SingleOrDefaultAsync(
            i => i.ProductId == request.ProductId,
            cancellationToken)
            ?? throw new NotFoundException(nameof(Inventory), request.ProductId);

        inventory.AdjustStock(request.NewQuantity, request.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
