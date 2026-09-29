using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Payments.Dtos;
using MediatR;

namespace Application.Features.Payments.Queries.GetPayments;

public sealed record GetPaymentsQuery(PaginationRequest Pagination) : IRequest<PagedResult<PaymentDto>>;
