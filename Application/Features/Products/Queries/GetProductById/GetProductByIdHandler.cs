using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Products.Dtos;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

internal class GetProductByIdHandler :
    IRequestHandler<GetProductByIdQuery, DetailedProduct>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<DetailedProduct> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository
            .ProjectToSingleOrDefaultAsync<DetailedProduct>(p => p.Id == request.Id && p.IsActive, cancellationToken);

        if (product is null)
            throw new NotFoundException($"Product with Id {request.Id} not found");

        return product;
    }
}
