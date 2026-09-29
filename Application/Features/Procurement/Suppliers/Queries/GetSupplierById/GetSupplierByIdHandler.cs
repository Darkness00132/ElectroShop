using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Procurement.Suppliers.Dtos;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.Suppliers.Queries.GetSupplierById;

internal class GetSupplierByIdHandler : IRequestHandler<GetSupplierByIdQuery, SupplierDto>
{
    private readonly IRepository<Supplier> _supplierRepository;

    public GetSupplierByIdHandler(IRepository<Supplier> supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<SupplierDto> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.ProjectToSingleOrDefaultAsync<SupplierDto>(
            s => s.Id == request.Id,
            cancellationToken);

        if (supplier is null)
            throw new NotFoundException(nameof(Supplier), request.Id);

        return supplier;
    }
}
