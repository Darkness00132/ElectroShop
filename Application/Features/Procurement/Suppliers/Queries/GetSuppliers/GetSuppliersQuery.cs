using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Procurement.Suppliers.Dtos;
using MediatR;

namespace Application.Features.Procurement.Suppliers.Queries.GetSuppliers;

public sealed record GetSuppliersQuery(PaginationRequest Pagination) : IRequest<PagedResult<SupplierDto>>;
