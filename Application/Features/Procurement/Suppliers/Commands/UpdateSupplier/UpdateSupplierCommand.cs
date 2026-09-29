using MediatR;

namespace Application.Features.Procurement.Suppliers.Commands.UpdateSupplier;

public sealed record UpdateSupplierCommand(
    Guid Id,
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? TaxNumber) : IRequest;
