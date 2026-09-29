using MediatR;

namespace Application.Features.Procurement.Suppliers.Commands.ActivateSupplier;

public sealed record ActivateSupplierCommand(Guid Id) : IRequest;
