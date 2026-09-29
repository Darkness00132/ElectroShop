using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.PromoCodes.Commands.ActivatePromoCode;
using Application.Features.PromoCodes.Commands.CreatePromoCode;
using Application.Features.PromoCodes.Commands.DeactivatePromoCode;
using Application.Features.PromoCodes.Commands.DeletePromoCode;
using Application.Features.PromoCodes.Commands.UpdatePromoCode;
using Application.Features.PromoCodes.Queries.GetPromoCodeById;
using Application.Features.PromoCodes.Queries.GetPromoCodes;
using Domain.Constants;
using Ecommerce.Api.Contracts.PromoCodes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

/// <summary>
/// Provides sales staff endpoints for managing promo codes.
/// </summary>
[ApiController]
[Authorize(Roles = AppRoles.SalesAdministrators)]
[Route("api/admin/promo-codes")]
[Produces("application/json")]
public sealed class PromoCodeManagementController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Creates a new promo code and returns its identifier.
    /// </summary>
    /// <response code="201">The promo code was created successfully.</response>
    /// <response code="400">The supplied promo code data is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a promo code management role.</response>
    /// <response code="409">A promo code with the same code already exists.</response>
    /// <param name="request">The promo code creation details.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The identifier of the created promo code.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreatePromoCode(
        CreatePromoCodeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePromoCodeCommand(
            request.Code,
            request.DiscountType,
            request.Value,
            request.MinimumOrder,
            request.StartDate,
            request.EndDate,
            request.UsageLimit);

        var promoCodeId = await sender.Send(command, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(GetPromoCodeById),
            routeValues: new { id = promoCodeId },
            value: promoCodeId);
    }

    /// <summary>
    /// Retrieves all promo codes, ordered by start date.
    /// </summary>
    /// <response code="200">The promo codes were retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a promo code management role.</response>
    /// <param name="pagination">The pagination options.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A paginated collection of promo codes.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Application.Features.PromoCodes.Dtos.PromoCodeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<Application.Features.PromoCodes.Dtos.PromoCodeDto>>> GetPromoCodes(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var promoCodes = await sender.Send(new GetPromoCodesQuery(pagination), cancellationToken);

        return Ok(promoCodes);
    }

    /// <summary>
    /// Retrieves a single promo code by its identifier.
    /// </summary>
    /// <response code="200">The promo code was retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a promo code management role.</response>
    /// <response code="404">No promo code exists with the supplied identifier.</response>
    /// <param name="id">The promo code identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The promo code.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Application.Features.PromoCodes.Dtos.PromoCodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Application.Features.PromoCodes.Dtos.PromoCodeDto>> GetPromoCodeById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var promoCode = await sender.Send(new GetPromoCodeByIdQuery(id), cancellationToken);

        return Ok(promoCode);
    }

    /// <summary>
    /// Partially updates a promo code. Omitted fields keep their current values.
    /// </summary>
    /// <response code="204">The promo code was updated successfully.</response>
    /// <response code="400">The supplied promo code data is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a promo code management role.</response>
    /// <response code="404">No promo code exists with the supplied identifier.</response>
    /// <response code="409">A promo code with the same code already exists.</response>
    /// <param name="id">The promo code identifier.</param>
    /// <param name="request">The promo code update details.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the promo code is updated.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdatePromoCode(
        Guid id,
        UpdatePromoCodeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePromoCodeCommand(
            id,
            request.Code,
            request.DiscountType,
            request.Value,
            request.MinimumOrder,
            request.StartDate,
            request.EndDate,
            request.UsageLimit,
            request.IsActive);

        await sender.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Makes a promo code usable again.
    /// </summary>
    /// <response code="204">The promo code was activated successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a promo code management role.</response>
    /// <response code="404">No promo code exists with the supplied identifier.</response>
    /// <param name="id">The promo code identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the promo code is activated.</returns>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivatePromoCode(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new ActivatePromoCodeCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Makes a promo code unusable without deleting it.
    /// </summary>
    /// <response code="204">The promo code was deactivated successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a promo code management role.</response>
    /// <response code="404">No promo code exists with the supplied identifier.</response>
    /// <param name="id">The promo code identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the promo code is deactivated.</returns>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivatePromoCode(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeactivatePromoCodeCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a promo code. Orders that used it keep their recorded discount.
    /// </summary>
    /// <response code="204">The promo code was deleted successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a promo code management role.</response>
    /// <response code="404">No promo code exists with the supplied identifier.</response>
    /// <param name="id">The promo code identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the promo code is deleted.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePromoCode(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeletePromoCodeCommand(id), cancellationToken);

        return NoContent();
    }
}
