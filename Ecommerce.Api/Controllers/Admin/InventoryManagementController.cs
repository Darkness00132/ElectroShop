using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Inventories.Commands.AdjustStock;
using Application.Features.Inventories.Commands.ChangeReorderLevel;
using Application.Features.Inventories.Commands.StockIn;
using Application.Features.Inventories.Commands.StockOut;
using Application.Features.Inventories.Queries.GetInventories;
using Application.Features.Inventories.Queries.GetInventoryByProductId;
using Domain.Constants;
using Ecommerce.Api.Contracts.Inventories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

/// <summary>
/// Provides inventory staff endpoints for managing product stock.
/// </summary>
/// <remarks>
/// Every stock change is recorded as an inventory transaction with the quantity before and after.
/// </remarks>
[ApiController]
[Authorize(Roles = AppRoles.InventoryAdministrators)]
[Route("api/admin/inventories")]
[Produces("application/json")]
public sealed class InventoryManagementController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all inventories ordered by quantity on hand.
    /// </summary>
    /// <response code="200">The inventories were retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to an inventory management role.</response>
    /// <param name="pagination">The pagination options.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A paginated collection of inventories.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Application.Features.Inventories.Dtos.InventoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<Application.Features.Inventories.Dtos.InventoryDto>>> GetInventories(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var inventories = await sender.Send(new GetInventoriesQuery(pagination), cancellationToken);

        return Ok(inventories);
    }

    /// <summary>
    /// Retrieves the inventory of a single product.
    /// </summary>
    /// <response code="200">The inventory was retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to an inventory management role.</response>
    /// <response code="404">No inventory exists for the supplied product.</response>
    /// <param name="productId">The product identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The product inventory.</returns>
    [HttpGet("products/{productId}")]
    [ProducesResponseType(typeof(Application.Features.Inventories.Dtos.InventoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Application.Features.Inventories.Dtos.InventoryDto>> GetInventoryByProductId(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var inventory = await sender.Send(new GetInventoryByProductIdQuery(productId), cancellationToken);

        return Ok(inventory);
    }

    /// <summary>
    /// Receives stock into the inventory of a product.
    /// </summary>
    /// <response code="204">The stock was received successfully.</response>
    /// <response code="400">The supplied quantity is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to an inventory management role.</response>
    /// <response code="404">No inventory exists for the supplied product.</response>
    /// <param name="productId">The product identifier.</param>
    /// <param name="request">The quantity and optional notes.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the stock is received.</returns>
    [HttpPost("products/{productId}/stock-in")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StockIn(
        Guid productId,
        StockInRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new StockInCommand(productId, request.Quantity, request.Notes), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Removes stock from the inventory of a product.
    /// </summary>
    /// <response code="204">The stock was removed successfully.</response>
    /// <response code="400">The quantity exceeds the stock on hand.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to an inventory management role.</response>
    /// <response code="404">No inventory exists for the supplied product.</response>
    /// <param name="productId">The product identifier.</param>
    /// <param name="request">The quantity and optional notes.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the stock is removed.</returns>
    [HttpPost("products/{productId}/stock-out")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StockOut(
        Guid productId,
        StockOutRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new StockOutCommand(productId, request.Quantity, request.Notes), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Adjusts the stock of a product to an absolute quantity, for example after a stock count.
    /// </summary>
    /// <response code="204">The stock was adjusted successfully.</response>
    /// <response code="400">The supplied quantity is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to an inventory management role.</response>
    /// <response code="404">No inventory exists for the supplied product.</response>
    /// <param name="productId">The product identifier.</param>
    /// <param name="request">The new quantity and optional notes.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the stock is adjusted.</returns>
    [HttpPost("products/{productId}/adjust")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdjustStock(
        Guid productId,
        AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new AdjustStockCommand(productId, request.NewQuantity, request.Notes), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Changes the reorder level of a product.
    /// </summary>
    /// <response code="204">The reorder level was changed successfully.</response>
    /// <response code="400">The supplied reorder level is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to an inventory management role.</response>
    /// <response code="404">No inventory exists for the supplied product.</response>
    /// <param name="productId">The product identifier.</param>
    /// <param name="request">The new reorder level.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the reorder level is changed.</returns>
    [HttpPut("products/{productId}/reorder-level")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeReorderLevel(
        Guid productId,
        ChangeReorderLevelRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ChangeReorderLevelCommand(productId, request.ReorderLevel), cancellationToken);

        return NoContent();
    }
}
