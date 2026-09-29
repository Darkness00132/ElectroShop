using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.Suppliers.Commands.UpdateSupplier;

internal class UpdateSupplierHandler : IRequestHandler<UpdateSupplierCommand>
{
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSupplierHandler(IRepository<Supplier> supplierRepository, IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Supplier), request.Id);

        supplier.Update(
            request.Name,
            request.ContactName,
            request.Email,
            request.Phone,
            request.Address,
            request.City,
            request.TaxNumber);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
