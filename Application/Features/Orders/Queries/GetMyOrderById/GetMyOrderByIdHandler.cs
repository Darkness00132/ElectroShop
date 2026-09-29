using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Application.Features.Orders.Dtos;
using Domain.Entities.OrdersAggregate;
using MediatR;

namespace Application.Features.Orders.Queries.GetMyOrderById;

internal class GetMyOrderByIdHandler : IRequestHandler<GetMyOrderByIdQuery, OrderDto>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly ICurrentUserService _currentUser;

    public GetMyOrderByIdHandler(IRepository<Order> orderRepository, ICurrentUserService currentUser)
    {
        _orderRepository = orderRepository;
        _currentUser = currentUser;
    }

    public async Task<OrderDto> Handle(GetMyOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.ProjectToSingleOrDefaultAsync<OrderDto>(
            o => o.Id == request.Id && o.UserId == _currentUser.UserId,
            cancellationToken);

        if (order is null)
            throw new NotFoundException(nameof(Order), request.Id);

        return order;
    }
}
