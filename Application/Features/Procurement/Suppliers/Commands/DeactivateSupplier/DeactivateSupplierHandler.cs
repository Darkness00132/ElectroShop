using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities.ProcurementAggregate;
using MediatR;

namespace Application.Features.Procurement.Suppliers.Commands.DeactivateSupplier;

internal class DeactivateSupplierHandler : IRequestHandler<DeactivateSupplierCommand>
{
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateSupplierHandler(IRepository<Supplier> supplierRepository, IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Supplier), request.Id);

        supplier.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
