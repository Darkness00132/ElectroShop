using MediatR;

namespace Application.Features.Procurement.Suppliers.Commands.CreateSupplier;

public sealed record CreateSupplierCommand(
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? TaxNumber) : IRequest<Guid>;
