using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Procurement.GoodsReceipts.Commands.AddGoodsReceiptItem;
using Application.Features.Procurement.GoodsReceipts.Commands.CancelGoodsReceipt;
using Application.Features.Procurement.GoodsReceipts.Commands.ConfirmGoodsReceipt;
using Application.Features.Procurement.GoodsReceipts.Commands.CreateGoodsReceipt;
using Application.Features.Procurement.GoodsReceipts.Queries.GetGoodsReceiptById;
using Application.Features.Procurement.GoodsReceipts.Queries.GetGoodsReceipts;
using Domain.Constants;
using Ecommerce.Api.Contracts.Procurement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

/// <summary>
/// Provides procurement staff endpoints for managing goods receipts.
/// </summary>
/// <remarks>
/// Confirming a goods receipt records the received quantities on the purchase order and
/// increases the product stock through auditable inventory transactions.
/// </remarks>
[ApiController]
[Authorize(Roles = AppRoles.ProcurementAdministrators)]
[Route("api/admin/goods-receipts")]
[Produces("application/json")]
public sealed class GoodsReceiptManagementController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Creates a goods receipt in draft state for a purchase order and returns its identifier.
    /// </summary>
    /// <response code="201">The goods receipt was created successfully.</response>
    /// <response code="400">The supplied data is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">The purchase order does not exist.</response>
    /// <response code="409">A goods receipt with the same number already exists.</response>
    /// <param name="request">The goods receipt creation details.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The identifier of the created goods receipt.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateGoodsReceipt(
        CreateGoodsReceiptRequest request,
        CancellationToken cancellationToken)
    {
        var goodsReceiptId = await sender.Send(new CreateGoodsReceiptCommand(
            request.Number,
            request.PurchaseOrderId,
            request.ReceivedAt,
            request.DeliveryReference,
            request.Notes), cancellationToken);

        return CreatedAtAction(
            actionName: nameof(GetGoodsReceiptById),
            routeValues: new { id = goodsReceiptId },
            value: goodsReceiptId);
    }

    /// <summary>
    /// Retrieves all goods receipts, newest first.
    /// </summary>
    /// <response code="200">The goods receipts were retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <param name="pagination">The pagination options.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A paginated collection of goods receipts.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Application.Features.Procurement.GoodsReceipts.Dtos.GoodsReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<Application.Features.Procurement.GoodsReceipts.Dtos.GoodsReceiptDto>>> GetGoodsReceipts(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var goodsReceipts = await sender.Send(new GetGoodsReceiptsQuery(pagination), cancellationToken);

        return Ok(goodsReceipts);
    }

    /// <summary>
    /// Retrieves a single goods receipt with its items.
    /// </summary>
    /// <response code="200">The goods receipt was retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">No goods receipt exists with the supplied identifier.</response>
    /// <param name="id">The goods receipt identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The goods receipt.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Application.Features.Procurement.GoodsReceipts.Dtos.GoodsReceiptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Application.Features.Procurement.GoodsReceipts.Dtos.GoodsReceiptDto>> GetGoodsReceiptById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var goodsReceipt = await sender.Send(new GetGoodsReceiptByIdQuery(id), cancellationToken);

        return Ok(goodsReceipt);
    }

    /// <summary>
    /// Adds a received product line to a draft goods receipt.
    /// </summary>
    /// <response code="204">The item was added successfully.</response>
    /// <response code="400">The item data is invalid or the receipt is not in draft.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">The goods receipt or product does not exist.</response>
    /// <param name="id">The goods receipt identifier.</param>
    /// <param name="request">The product and received quantity.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the item is added.</returns>
    [HttpPost("{id}/items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddGoodsReceiptItem(
        Guid id,
        AddGoodsReceiptItemRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new AddGoodsReceiptItemCommand(id, request.ProductId, request.Quantity), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Confirms a draft goods receipt: records the received quantities on the purchase order
    /// and increases the product stock.
    /// </summary>
    /// <response code="204">The goods receipt was confirmed successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">No goods receipt exists with the supplied identifier.</response>
    /// <response code="409">The receipt contains a product that does not belong to the purchase order,
    /// or the received quantity exceeds the ordered quantity.</response>
    /// <param name="id">The goods receipt identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the goods receipt is confirmed.</returns>
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmGoodsReceipt(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new ConfirmGoodsReceiptCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Cancels a draft goods receipt. Confirmed receipts cannot be cancelled.
    /// </summary>
    /// <response code="204">The goods receipt was cancelled successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">No goods receipt exists with the supplied identifier.</response>
    /// <response code="409">Only draft goods receipts can be cancelled.</response>
    /// <param name="id">The goods receipt identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the goods receipt is cancelled.</returns>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelGoodsReceipt(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new CancelGoodsReceiptCommand(id), cancellationToken);

        return NoContent();
    }
}
