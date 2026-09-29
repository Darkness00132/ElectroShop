using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Procurement.PurchaseOrders.Commands.AddPurchaseOrderItem;
using Application.Features.Procurement.PurchaseOrders.Commands.ApprovePurchaseOrder;
using Application.Features.Procurement.PurchaseOrders.Commands.CancelPurchaseOrder;
using Application.Features.Procurement.PurchaseOrders.Commands.CompletePurchaseOrder;
using Application.Features.Procurement.PurchaseOrders.Commands.CreatePurchaseOrder;
using Application.Features.Procurement.PurchaseOrders.Commands.SetPurchaseOrderCosts;
using Application.Features.Procurement.PurchaseOrders.Commands.SubmitPurchaseOrder;
using Application.Features.Procurement.PurchaseOrders.Queries.GetPurchaseOrderById;
using Application.Features.Procurement.PurchaseOrders.Queries.GetPurchaseOrders;
using Domain.Constants;
using Ecommerce.Api.Contracts.Procurement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

/// <summary>
/// Provides procurement staff endpoints for managing purchase orders through the
/// Draft → PendingApproval → Approved → PartiallyReceived → Completed or Cancelled flow.
/// </summary>
[ApiController]
[Authorize(Roles = AppRoles.ProcurementAdministrators)]
[Route("api/admin/purchase-orders")]
[Produces("application/json")]
public sealed class PurchaseOrderManagementController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Creates a purchase order in draft state and returns its identifier.
    /// </summary>
    /// <response code="201">The purchase order was created successfully.</response>
    /// <response code="400">The supplied data is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">The supplier does not exist.</response>
    /// <response code="409">A purchase order with the same number already exists.</response>
    /// <param name="request">The purchase order creation details.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The identifier of the created purchase order.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreatePurchaseOrder(
        CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        var purchaseOrderId = await sender.Send(new CreatePurchaseOrderCommand(
            request.Number,
            request.SupplierId,
            request.OrderDate,
            request.ExpectedDeliveryDate,
            request.Notes), cancellationToken);

        return CreatedAtAction(
            actionName: nameof(GetPurchaseOrderById),
            routeValues: new { id = purchaseOrderId },
            value: purchaseOrderId);
    }

    /// <summary>
    /// Retrieves all purchase orders, newest first.
    /// </summary>
    /// <response code="200">The purchase orders were retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <param name="pagination">The pagination options.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A paginated collection of purchase orders.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Application.Features.Procurement.PurchaseOrders.Dtos.PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<Application.Features.Procurement.PurchaseOrders.Dtos.PurchaseOrderDto>>> GetPurchaseOrders(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var purchaseOrders = await sender.Send(new GetPurchaseOrdersQuery(pagination), cancellationToken);

        return Ok(purchaseOrders);
    }

    /// <summary>
    /// Retrieves a single purchase order with its items.
    /// </summary>
    /// <response code="200">The purchase order was retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">No purchase order exists with the supplied identifier.</response>
    /// <param name="id">The purchase order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The purchase order.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Application.Features.Procurement.PurchaseOrders.Dtos.PurchaseOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Application.Features.Procurement.PurchaseOrders.Dtos.PurchaseOrderDto>> GetPurchaseOrderById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await sender.Send(new GetPurchaseOrderByIdQuery(id), cancellationToken);

        return Ok(purchaseOrder);
    }

    /// <summary>
    /// Adds a product line to a draft purchase order.
    /// </summary>
    /// <response code="204">The item was added successfully.</response>
    /// <response code="400">The supplied item data is invalid or the order is not in draft.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">The purchase order or product does not exist.</response>
    /// <param name="id">The purchase order identifier.</param>
    /// <param name="request">The product, quantity, and unit cost.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the item is added.</returns>
    [HttpPost("{id}/items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPurchaseOrderItem(
        Guid id,
        AddPurchaseOrderItemRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new AddPurchaseOrderItemCommand(id, request.ProductId, request.OrderedQuantity, request.UnitCost), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Sets the tax and shipping costs of a draft purchase order.
    /// </summary>
    /// <response code="204">The costs were set successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">No purchase order exists with the supplied identifier.</response>
    /// <param name="id">The purchase order identifier.</param>
    /// <param name="request">The tax amount and shipping cost.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the costs are set.</returns>
    [HttpPut("{id}/costs")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPurchaseOrderCosts(
        Guid id,
        SetPurchaseOrderCostsRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new SetPurchaseOrderCostsCommand(id, request.TaxAmount, request.ShippingCost), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Submits a draft purchase order for approval. The order must contain items.
    /// </summary>
    /// <response code="204">The purchase order was submitted successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">No purchase order exists with the supplied identifier.</response>
    /// <response code="409">Only draft purchase orders with items can be submitted.</response>
    /// <param name="id">The purchase order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the purchase order is submitted.</returns>
    [HttpPost("{id}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitPurchaseOrder(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new SubmitPurchaseOrderCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Approves a purchase order that is pending approval.
    /// </summary>
    /// <response code="204">The purchase order was approved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">No purchase order exists with the supplied identifier.</response>
    /// <response code="409">Only pending approval purchase orders can be approved.</response>
    /// <param name="id">The purchase order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the purchase order is approved.</returns>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ApprovePurchaseOrder(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new ApprovePurchaseOrderCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Completes an approved or partially received purchase order.
    /// </summary>
    /// <response code="204">The purchase order was completed successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">No purchase order exists with the supplied identifier.</response>
    /// <response code="409">Only approved or partially received purchase orders can be completed.</response>
    /// <param name="id">The purchase order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the purchase order is completed.</returns>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CompletePurchaseOrder(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new CompletePurchaseOrderCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Cancels a purchase order that has not been completed.
    /// </summary>
    /// <response code="204">The purchase order was cancelled successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">No purchase order exists with the supplied identifier.</response>
    /// <response code="409">Completed or already cancelled purchase orders cannot be cancelled.</response>
    /// <param name="id">The purchase order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the purchase order is cancelled.</returns>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelPurchaseOrder(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new CancelPurchaseOrderCommand(id), cancellationToken);

        return NoContent();
    }
}
