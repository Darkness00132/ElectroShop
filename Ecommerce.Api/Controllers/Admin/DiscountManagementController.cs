using Application.Features.Discounts.Commands.CreateDiscount;
using Application.Features.Discounts.Commands.DeleteDiscount;
using Application.Features.Discounts.Commands.UpdateDiscount;
using Application.Features.Discounts.Common;
using Application.Features.Discounts.Queries.GetDiscount;
using Application.Features.Discounts.Queries.GetDiscounts;
using AutoMapper;
using Domain.Constants;
using Ecommerce.Api.Contracts.Discount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

/// <summary>
/// Provides catalog administrator endpoints for managing discounts.
/// </summary>
/// <remarks>
/// Discounts are managed independently and can later be assigned to products.
/// The list endpoint returns all discounts, including active, inactive, scheduled, and expired ones.
/// </remarks>
[ApiController]
[Authorize(Roles = AppRoles.CatalogAdministrators)]
[Route("api/admin/discounts")]
[Produces("application/json")]
public sealed class DiscountManagementController(IMapper mapper, ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all discounts.
    /// </summary>
    /// <remarks>
    /// Returns all discounts in the system, including active, inactive, scheduled, and expired discounts.
    /// </remarks>
    /// <response code="200">The discounts were retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a discount management role.</response>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The collection of discounts.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<DiscountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyCollection<DiscountDto>>> GetDiscounts(
        CancellationToken cancellationToken)
    {
        var discounts = await sender.Send(new GetDiscountsQuery(), cancellationToken);

        return Ok(discounts);
    }

    /// <summary>
    /// Retrieves a single discount by its identifier.
    /// </summary>
    /// <response code="200">The discount was retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a discount management role.</response>
    /// <response code="404">No discount exists with the supplied identifier.</response>
    /// <param name="id">The discount identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The requested discount.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DiscountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiscountDto>> GetDiscountById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var discount = await sender.Send(new GetDiscountQuery(id), cancellationToken);

        return Ok(discount);
    }

    /// <summary>
    /// Creates a new discount and returns its identifier.
    /// </summary>
    /// <remarks>
    /// Discounts can be created as either percentage-based or fixed-amount discounts.
    /// For percentage discounts the value is 1-100; for fixed-amount discounts it is the absolute amount.
    /// The validity period determines when the discount can be applied, and an inactive discount is
    /// never visible or applicable until it is activated.
    /// </remarks>
    /// <response code="201">The discount was created successfully.</response>
    /// <response code="400">The supplied discount data is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a discount management role.</response>
    /// <param name="command">The discount creation details.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The identifier of the created discount.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> CreateDiscount(
        CreateDiscountCommand command,
        CancellationToken cancellationToken)
    {
        var discountId = await sender.Send(command, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(GetDiscountById),
            routeValues: new { id = discountId },
            value: discountId);
    }

    /// <summary>
    /// Partially updates an existing discount. Omitted fields keep their current values.
    /// </summary>
    /// <response code="204">The discount was updated successfully.</response>
    /// <response code="400">The supplied discount data is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a discount management role.</response>
    /// <response code="404">No discount exists with the supplied identifier.</response>
    /// <param name="id">The discount identifier.</param>
    /// <param name="request">The discount update details.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the discount is updated.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDiscount(
        Guid id,
        UpdateDiscountRequest request,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<UpdateDiscountCommand>(request) with { Id = id };

        await sender.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a discount.
    /// </summary>
    /// <remarks>
    /// Any products currently associated with this discount will have the discount
    /// removed before the discount itself is deleted.
    /// </remarks>
    /// <response code="204">The discount was deleted successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a discount management role.</response>
    /// <response code="404">No discount exists with the supplied identifier.</response>
    /// <param name="id">The discount identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the discount is deleted.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDiscount(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteDiscountCommand(id), cancellationToken);

        return NoContent();
    }
}
