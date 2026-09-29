using MediatR;

namespace Application.Features.Procurement.Suppliers.Commands.DeactivateSupplier;

public sealed record DeactivateSupplierCommand(Guid Id) : IRequest;
