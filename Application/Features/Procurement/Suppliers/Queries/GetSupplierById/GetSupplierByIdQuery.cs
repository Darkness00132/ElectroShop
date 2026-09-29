using Application.Features.Procurement.Suppliers.Dtos;
using MediatR;

namespace Application.Features.Procurement.Suppliers.Queries.GetSupplierById;

public sealed record GetSupplierByIdQuery(Guid Id) : IRequest<SupplierDto>;
