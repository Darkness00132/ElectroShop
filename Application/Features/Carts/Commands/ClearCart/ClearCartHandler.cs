using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Domain.Entities.Carts;
using MediatR;

namespace Application.Features.Carts.Commands.ClearCart;

internal class ClearCartHandler : IRequestHandler<ClearCartCommand>
{
    private readonly IRepository<Cart> _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ClearCartHandler(IRepository<Cart> cartRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.SingleOrDefaultAsync(
            c => c.UserId == _currentUser.UserId,
            cancellationToken,
            c => c.Items);

        if (cart is null)
            return;

        cart.Clear();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
