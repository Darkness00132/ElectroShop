using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Orders.Commands.AdminCancelOrder;
using Application.Features.Orders.Commands.ConfirmOrder;
using Application.Features.Orders.Commands.DeliverOrder;
using Application.Features.Orders.Commands.ShipOrder;
using Application.Features.Orders.Commands.StartProcessingOrder;
using Application.Features.Orders.Queries.GetOrderById;
using Application.Features.Orders.Queries.GetOrders;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

/// <summary>
/// Provides sales staff endpoints for viewing orders and driving them through the
/// Pending → Processing → Shipped → Delivered or Cancelled state flow.
/// </summary>
[ApiController]
[Authorize(Roles = AppRoles.SalesAdministrators)]
[Route("api/admin/orders")]
[Produces("application/json")]
public sealed class OrderManagementController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated collection of all orders, newest first.
    /// </summary>
    /// <response code="200">The orders were retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to an order management role.</response>
    /// <param name="pagination">The pagination options.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A paginated collection of orders.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Application.Features.Orders.Dtos.OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<Application.Features.Orders.Dtos.OrderSummaryDto>>> GetOrders(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var orders = await sender.Send(new GetOrdersQuery(pagination), cancellationToken);

        return Ok(orders);
    }

    /// <summary>
    /// Retrieves a single order by its identifier.
    /// </summary>
    /// <response code="200">The order was retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to an order management role.</response>
    /// <response code="404">No order exists with the supplied identifier.</response>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The order.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Application.Features.Orders.Dtos.OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Application.Features.Orders.Dtos.OrderDto>> GetOrderById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await sender.Send(new GetOrderByIdQuery(id), cancellationToken);

        return Ok(order);
    }

    /// <summary>
    /// Confirms a pending order.
    /// </summary>
    /// <response code="204">The order was confirmed successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to an order management role.</response>
    /// <response code="404">No order exists with the supplied identifier.</response>
    /// <response code="409">Only pending orders can be confirmed.</response>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the order is confirmed.</returns>
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmOrder(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new ConfirmOrderCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Starts processing a confirmed order.
    /// </summary>
    /// <response code="204">The order was moved to processing successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to an order management role.</response>
    /// <response code="404">No order exists with the supplied identifier.</response>
    /// <response code="409">Only confirmed orders can be processed.</response>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the order is moved to processing.</returns>
    [HttpPost("{id}/process")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartProcessingOrder(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new StartProcessingOrderCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Marks a processing order as shipped.
    /// </summary>
    /// <response code="204">The order was shipped successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to an order management role.</response>
    /// <response code="404">No order exists with the supplied identifier.</response>
    /// <response code="409">Only processing orders can be shipped.</response>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the order is shipped.</returns>
    [HttpPost("{id}/ship")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ShipOrder(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new ShipOrderCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Marks a shipped order as delivered. For cash on delivery, collect the payment before delivering.
    /// </summary>
    /// <response code="204">The order was delivered successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to an order management role.</response>
    /// <response code="404">No order exists with the supplied identifier.</response>
    /// <response code="409">Only shipped orders can be delivered.</response>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the order is delivered.</returns>
    [HttpPost("{id}/deliver")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeliverOrder(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeliverOrderCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Cancels an order that has not been delivered. The reserved stock is returned to the inventory.
    /// </summary>
    /// <response code="204">The order was cancelled successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to an order management role.</response>
    /// <response code="404">No order exists with the supplied identifier.</response>
    /// <response code="409">Delivered or refunded orders cannot be cancelled.</response>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the order is cancelled.</returns>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new AdminCancelOrderCommand(id), cancellationToken);

        return NoContent();
    }
}
