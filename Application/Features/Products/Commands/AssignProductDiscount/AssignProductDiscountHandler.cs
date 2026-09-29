using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.Catalog;
using MediatR;

namespace Application.Features.Products.Commands.AssignProductDiscount;

internal class AssignProductDiscountHandler : IRequestHandler<AssignProductDiscountCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IRepository<Discount> _discountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignProductDiscountHandler(IProductRepository productRepository, IRepository<Discount> discountRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _discountRepository = discountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AssignProductDiscountCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var discount = await _discountRepository.GetByIdAsync(request.DiscountId, cancellationToken)
            ?? throw new NotFoundException(nameof(Discount), request.DiscountId);

        product.AssignDiscount(discount);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
