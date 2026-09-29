using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Domain.Entities.Carts;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using MediatR;

namespace Application.Features.Carts.Commands.UpdateCartItem;

internal class UpdateCartItemHandler : IRequestHandler<UpdateCartItemCommand>
{
    private readonly IRepository<Cart> _cartRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateCartItemHandler(IRepository<Cart> cartRepository, IRepository<Product> productRepository, IRepository<Inventory> inventoryRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.SingleOrDefaultAsync(
            c => c.UserId == _currentUser.UserId,
            cancellationToken,
            c => c.Items)
            ?? throw new NotFoundException(nameof(CartItem), request.ProductId);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId)
            ?? throw new NotFoundException(nameof(CartItem), request.ProductId);

        var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), item.ProductId);

        if (!product.IsActive)
            throw new ConflictException("The product is not available.");

        var inventory = await _inventoryRepository
            .SingleOrDefaultAsync(i => i.ProductId == product.Id, cancellationToken);

        if (inventory is null || inventory.QuantityOnHand < request.Quantity)
            throw new ConflictException("The requested quantity exceeds the available stock.");

        cart.UpdateItemQuantity(request.ProductId, request.Quantity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
