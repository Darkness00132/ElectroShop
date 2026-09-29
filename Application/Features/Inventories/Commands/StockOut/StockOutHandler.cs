using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.InventoryAggregate;
using MediatR;

namespace Application.Features.Inventories.Commands.StockOut;

internal class StockOutHandler : IRequestHandler<StockOutCommand>
{
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StockOutHandler(IRepository<Inventory> inventoryRepository, IUnitOfWork unitOfWork)
    {
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(StockOutCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.SingleOrDefaultAsync(
            i => i.ProductId == request.ProductId,
            cancellationToken)
            ?? throw new NotFoundException(nameof(Inventory), request.ProductId);

        inventory.DecreaseStock(request.Quantity, notes: request.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
