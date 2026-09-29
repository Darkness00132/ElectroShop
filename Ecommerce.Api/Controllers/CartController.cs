using Application.Features.Carts.Commands.AddCartItem;
using Application.Features.Carts.Commands.ClearCart;
using Application.Features.Carts.Commands.RemoveCartItem;
using Application.Features.Carts.Commands.UpdateCartItem;
using Application.Features.Carts.Dtos;
using Application.Features.Carts.Queries.GetCheckoutSummary;
using Application.Features.Carts.Queries.GetMyCart;
using Ecommerce.Api.Contracts.Carts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

/// <summary>
/// Provides endpoints for the authenticated customer to manage their cart.
/// </summary>
/// <remarks>
/// Each customer has one persistent cart; adding a product that is already in the cart
/// increases its quantity.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/cart")]
[Produces("application/json")]
public sealed class CartController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves the cart of the currently authenticated customer.
    /// </summary>
    /// <remarks>
    /// The cart includes the current unit prices and any active discounts per line.
    /// Customers without a cart receive an empty cart.
    /// </remarks>
    /// <response code="200">The cart was retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The customer's cart.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CartDto>> GetMyCart(
        CancellationToken cancellationToken)
    {
        var cart = await sender.Send(new GetMyCartQuery(), cancellationToken);

        return Ok(cart);
    }

    /// <summary>
    /// Retrieves the checkout summary of the cart: live prices, active discounts,
    /// available stock, and the computed totals including the shipping fee.
    /// </summary>
    /// <response code="200">The checkout summary was retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="409">The cart is empty.</response>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The checkout summary.</returns>
    [HttpGet("checkout")]
    [ProducesResponseType(typeof(CheckoutSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CheckoutSummaryDto>> GetCheckoutSummary(
        CancellationToken cancellationToken)
    {
        var summary = await sender.Send(new GetCheckoutSummaryQuery(), cancellationToken);

        return Ok(summary);
    }

    /// <summary>
    /// Adds a product to the cart, or increases the quantity of an existing cart line.
    /// </summary>
    /// <remarks>
    /// The product must be active and the requested total quantity must not exceed the available stock.
    /// </remarks>
    /// <response code="204">The product was added to the cart successfully.</response>
    /// <response code="400">The supplied quantity is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="404">The product does not exist.</response>
    /// <response code="409">The product is not available or the stock is insufficient.</response>
    /// <param name="request">The product and quantity to add.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the product is added.</returns>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddCartItem(
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new AddCartItemCommand(request.ProductId, request.Quantity), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Changes the quantity of a product line in the cart.
    /// </summary>
    /// <remarks>
    /// The new quantity must not exceed the available stock.
    /// </remarks>
    /// <response code="204">The cart item was updated successfully.</response>
    /// <response code="400">The supplied quantity is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="404">The cart item does not exist.</response>
    /// <response code="409">The product is not available or the stock is insufficient.</response>
    /// <param name="productId">The identifier of the product line to update.</param>
    /// <param name="request">The new quantity.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the cart item is updated.</returns>
    [HttpPut("items/{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCartItem(
        Guid productId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateCartItemCommand(productId, request.Quantity), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Removes a product line from the cart.
    /// </summary>
    /// <response code="204">The cart item was removed successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="404">The cart item does not exist.</response>
    /// <param name="productId">The identifier of the product line to remove.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the cart item is removed.</returns>
    [HttpDelete("items/{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCartItem(
        Guid productId,
        CancellationToken cancellationToken)
    {
        await sender.Send(new RemoveCartItemCommand(productId), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Removes all items from the cart.
    /// </summary>
    /// <response code="204">The cart was cleared successfully, or was already empty.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the cart is cleared.</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ClearCart(
        CancellationToken cancellationToken)
    {
        await sender.Send(new ClearCartCommand(), cancellationToken);

        return NoContent();
    }
}
