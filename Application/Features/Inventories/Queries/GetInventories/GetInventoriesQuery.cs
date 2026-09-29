using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Inventories.Dtos;
using MediatR;

namespace Application.Features.Inventories.Queries.GetInventories;

public sealed record GetInventoriesQuery(PaginationRequest Pagination) : IRequest<PagedResult<InventoryDto>>;
