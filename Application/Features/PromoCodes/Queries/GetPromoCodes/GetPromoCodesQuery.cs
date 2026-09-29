using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.PromoCodes.Dtos;
using MediatR;

namespace Application.Features.PromoCodes.Queries.GetPromoCodes;

public sealed record GetPromoCodesQuery(PaginationRequest Pagination) : IRequest<PagedResult<PromoCodeDto>>;
