using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Payments.Commands.CreateOrderPayment;
using Application.Features.Payments.Commands.MarkPaymentPaid;
using Application.Features.Payments.Commands.RefundPayment;
using Application.Features.Payments.Queries.GetPaymentById;
using Application.Features.Payments.Queries.GetPayments;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

/// <summary>
/// Provides sales staff endpoints for managing order payments.
/// </summary>
/// <remarks>
/// Payments are cash on delivery: the payment is recorded on the order, marked as paid when
/// the cash is collected, and can be refunded while the order is delivered.
/// </remarks>
[ApiController]
[Authorize(Roles = AppRoles.SalesAdministrators)]
[Route("api/admin/payments")]
[Produces("application/json")]
public sealed class PaymentManagementController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Records a cash-on-delivery payment for an order.
    /// </summary>
    /// <response code="201">The payment was recorded successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a payment management role.</response>
    /// <response code="404">No order exists with the supplied identifier.</response>
    /// <response code="409">A payment already exists for this order.</response>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The identifier of the recorded payment.</returns>
    [HttpPost("orders/{orderId}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateOrderPayment(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var paymentId = await sender.Send(new CreateOrderPaymentCommand(orderId), cancellationToken);

        return CreatedAtAction(
            actionName: nameof(GetPaymentById),
            routeValues: new { id = paymentId },
            value: paymentId);
    }

    /// <summary>
    /// Retrieves all payments, newest first.
    /// </summary>
    /// <response code="200">The payments were retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a payment management role.</response>
    /// <param name="pagination">The pagination options.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A paginated collection of payments.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Application.Features.Payments.Dtos.PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<Application.Features.Payments.Dtos.PaymentDto>>> GetPayments(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var payments = await sender.Send(new GetPaymentsQuery(pagination), cancellationToken);

        return Ok(payments);
    }

    /// <summary>
    /// Retrieves a single payment by its identifier.
    /// </summary>
    /// <response code="200">The payment was retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a payment management role.</response>
    /// <response code="404">No payment exists with the supplied identifier.</response>
    /// <param name="id">The payment identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The payment.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Application.Features.Payments.Dtos.PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Application.Features.Payments.Dtos.PaymentDto>> GetPaymentById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var payment = await sender.Send(new GetPaymentByIdQuery(id), cancellationToken);

        return Ok(payment);
    }

    /// <summary>
    /// Marks a payment as paid. Marking an already paid payment changes nothing.
    /// </summary>
    /// <response code="204">The payment was marked as paid successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a payment management role.</response>
    /// <response code="404">No payment exists with the supplied identifier.</response>
    /// <param name="id">The payment identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the payment is marked as paid.</returns>
    [HttpPost("{id}/mark-paid")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkPaymentPaid(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new MarkPaymentPaidCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Refunds a paid payment and marks its order as refunded.
    /// </summary>
    /// <response code="204">The payment was refunded successfully.</response>
    /// <response code="400">Only paid payments of delivered orders can be refunded.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a payment management role.</response>
    /// <response code="404">No payment exists with the supplied identifier.</response>
    /// <param name="id">The payment identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the payment is refunded.</returns>
    [HttpPost("{id}/refund")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefundPayment(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new RefundPaymentCommand(id), cancellationToken);

        return NoContent();
    }
}
