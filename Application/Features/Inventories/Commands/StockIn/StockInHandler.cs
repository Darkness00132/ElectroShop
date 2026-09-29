using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.InventoryAggregate;
using MediatR;

namespace Application.Features.Inventories.Commands.StockIn;

internal class StockInHandler : IRequestHandler<StockInCommand>
{
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StockInHandler(IRepository<Inventory> inventoryRepository, IUnitOfWork unitOfWork)
    {
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(StockInCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.SingleOrDefaultAsync(
            i => i.ProductId == request.ProductId,
            cancellationToken)
            ?? throw new NotFoundException(nameof(Inventory), request.ProductId);

        inventory.IncreaseStock(request.Quantity, notes: request.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
