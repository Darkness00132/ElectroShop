using Application.Features.Orders.Commands.CancelMyOrder;
using Application.Features.Orders.Commands.PlaceOrder;
using Application.Features.Orders.Queries.GetMyOrderById;
using Application.Features.Orders.Queries.GetMyOrders;
using Ecommerce.Api.Contracts.Orders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

/// <summary>
/// Provides endpoints for the authenticated customer to place and view their orders.
/// </summary>
[ApiController]
[Authorize]
[Route("api/orders")]
[Produces("application/json")]
public sealed class OrdersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Places an order from the current contents of the customer's cart.
    /// </summary>
    /// <remarks>
    /// The order captures the product names, SKUs, and prices at purchase time, applies any
    /// active product discounts and the optional promo code, adds the shipping fee, and
    /// decreases the product stock. The cart is cleared afterwards.
    /// Payment is cash on delivery.
    /// </remarks>
    /// <response code="201">The order was placed successfully.</response>
    /// <response code="400">The supplied shipping address is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="409">The cart is empty, a product is unavailable, the stock is insufficient,
    /// the promo code is invalid, or another checkout modified the stock at the same time.</response>
    /// <param name="request">The shipping address and optional promo code.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The identifier of the placed order.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> PlaceOrder(
        PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PlaceOrderCommand(
            request.Street,
            request.City,
            request.Phone,
            request.Notes,
            request.PromoCode);

        var orderId = await sender.Send(command, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(GetMyOrderById),
            routeValues: new { id = orderId },
            value: orderId);
    }

    /// <summary>
    /// Retrieves the orders of the currently authenticated customer, newest first.
    /// </summary>
    /// <response code="200">The orders were retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The customer's orders.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Application.Features.Orders.Dtos.OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<Application.Features.Orders.Dtos.OrderSummaryDto>>> GetMyOrders(
        CancellationToken cancellationToken)
    {
        var orders = await sender.Send(new GetMyOrdersQuery(), cancellationToken);

        return Ok(orders);
    }

    /// <summary>
    /// Retrieves a single order of the currently authenticated customer.
    /// </summary>
    /// <response code="200">The order was retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="404">No order of this customer exists with the supplied identifier.</response>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The customer's order.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Application.Features.Orders.Dtos.OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Application.Features.Orders.Dtos.OrderDto>> GetMyOrderById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await sender.Send(new GetMyOrderByIdQuery(id), cancellationToken);

        return Ok(order);
    }

    /// <summary>
    /// Cancels an order of the currently authenticated customer before it is processed.
    /// The reserved stock is returned to the inventory.
    /// </summary>
    /// <response code="204">The order was cancelled successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="404">No order of this customer exists with the supplied identifier.</response>
    /// <response code="409">The order can no longer be cancelled.</response>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the order is cancelled.</returns>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelMyOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new CancelMyOrderCommand(id), cancellationToken);

        return NoContent();
    }
}
