using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Domain.Entities.Carts;
using MediatR;

namespace Application.Features.Carts.Commands.RemoveCartItem;

internal class RemoveCartItemHandler : IRequestHandler<RemoveCartItemCommand>
{
    private readonly IRepository<Cart> _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RemoveCartItemHandler(IRepository<Cart> cartRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.SingleOrDefaultAsync(
            c => c.UserId == _currentUser.UserId,
            cancellationToken,
            c => c.Items)
            ?? throw new NotFoundException(nameof(CartItem), request.ProductId);

        if (!cart.Items.Any(item => item.ProductId == request.ProductId))
            throw new NotFoundException(nameof(CartItem), request.ProductId);

        cart.RemoveItem(request.ProductId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
