using Application.Abstractions.Repositories;
using Application.Common.Pagination;
using Application.Features.Procurement.Suppliers.Dtos;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.Suppliers.Queries.GetSuppliers;

internal class GetSuppliersHandler : IRequestHandler<GetSuppliersQuery, PagedResult<SupplierDto>>
{
    private readonly IRepository<Supplier> _supplierRepository;

    public GetSuppliersHandler(IRepository<Supplier> supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<PagedResult<SupplierDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        return await _supplierRepository.ProjectToPagedAsync<SupplierDto>(
            request.Pagination,
            orderBy: supplier => supplier.Name,
            cancellationToken: cancellationToken);
    }
}
