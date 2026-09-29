using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Orders.Dtos;
using Domain.Entities.OrdersAggregate;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrderById;

internal class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IRepository<Order> _orderRepository;

    public GetOrderByIdHandler(IRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.ProjectToSingleOrDefaultAsync<OrderDto>(
            o => o.Id == request.Id,
            cancellationToken);

        if (order is null)
            throw new NotFoundException(nameof(Order), request.Id);

        return order;
    }
}
