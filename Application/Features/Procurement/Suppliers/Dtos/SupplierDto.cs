namespace Application.Features.Procurement.Suppliers.Dtos;

public sealed record SupplierDto(
    Guid Id,
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? TaxNumber,
    bool IsActive,
    DateTime CreatedAt);
