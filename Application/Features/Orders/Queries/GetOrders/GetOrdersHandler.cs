using Application.Abstractions.Repositories;
using Application.Common.Pagination;
using Application.Features.Orders.Dtos;
using Domain.Entities.OrdersAggregate;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrders;

internal class GetOrdersHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderSummaryDto>>
{
    private readonly IRepository<Order> _orderRepository;

    public GetOrdersHandler(IRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<OrderSummaryDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _orderRepository.ProjectToPagedAsync<OrderSummaryDto>(
            request.Pagination,
            orderBy: order => order.CreatedAt,
            descending: true,
            cancellationToken: cancellationToken);
    }
}
