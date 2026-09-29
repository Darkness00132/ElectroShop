using Application.Abstractions.Repositories;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.Suppliers.Commands.CreateSupplier;

internal class CreateSupplierHandler : IRequestHandler<CreateSupplierCommand, Guid>
{
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSupplierHandler(IRepository<Supplier> supplierRepository, IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = new Supplier(
            request.Name,
            request.ContactName,
            request.Email,
            request.Phone,
            request.Address,
            request.City,
            request.TaxNumber);

        await _supplierRepository.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return supplier.Id;
    }
}
