using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Features.Orders.Dtos;
using Domain.Entities.OrdersAggregate;
using MediatR;

namespace Application.Features.Orders.Queries.GetMyOrders;

internal class GetMyOrdersHandler : IRequestHandler<GetMyOrdersQuery, IReadOnlyList<OrderSummaryDto>>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly ICurrentUserService _currentUser;

    public GetMyOrdersHandler(IRepository<Order> orderRepository, ICurrentUserService currentUser)
    {
        _orderRepository = orderRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _orderRepository.ProjectToListAsync<OrderSummaryDto>(
            orderBy: order => order.CreatedAt,
            predicate: order => order.UserId == _currentUser.UserId,
            descending: true,
            cancellationToken: cancellationToken);
    }
}
