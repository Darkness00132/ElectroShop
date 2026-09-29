using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Domain.Entities.Carts;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using MediatR;

namespace Application.Features.Carts.Commands.AddCartItem;

internal class AddCartItemHandler : IRequestHandler<AddCartItemCommand>
{
    private readonly IRepository<Cart> _cartRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AddCartItemHandler(IRepository<Cart> cartRepository, IRepository<Product> productRepository, IRepository<Inventory> inventoryRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        if (!product.IsActive)
            throw new ConflictException("The product is not available.");

        var inventory = await _inventoryRepository
            .SingleOrDefaultAsync(i => i.ProductId == product.Id, cancellationToken)
            ?? throw new ConflictException("The product is out of stock.");

        var cart = await GetOrCreateCartAsync(userId, cancellationToken);

        var requestedQuantity = cart.Items
            .Where(item => item.ProductId == product.Id)
            .Sum(item => item.Quantity) + request.Quantity;

        if (inventory.QuantityOnHand < requestedQuantity)
            throw new ConflictException("The requested quantity exceeds the available stock.");

        cart.AddItem(product.Id, request.Quantity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.SingleOrDefaultAsync(
            c => c.UserId == userId,
            cancellationToken,
            c => c.Items);

        if (cart is not null)
            return cart;

        cart = new Cart(userId);
        await _cartRepository.AddAsync(cart, cancellationToken);

        return cart;
    }
}
