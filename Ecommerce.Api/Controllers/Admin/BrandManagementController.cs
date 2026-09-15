using Application.Features.Brands.Commands.CreateBrand;
using Application.Features.Brands.Commands.UpdateBrand;
using Domain.Constants;
using Ecommerce.Api.Contracts.Brands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

/// <summary>
/// Provides catalog administrator endpoints for managing brands.
/// </summary>
[ApiController]
[Authorize(Roles = AppRoles.CatalogAdministrators)]
[Route("api/admin/brands")]
[Produces("application/json")]
public sealed class BrandManagementController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Creates a new brand and returns its identifier.
    /// </summary>
    /// <response code="201">The brand was created successfully.</response>
    /// <response code="400">The supplied brand data is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a brand management role.</response>
    /// <response code="409">A brand with the same English or Arabic name already exists.</response>
    /// <param name="command">The brand creation details.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The identifier of the created brand.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateBrand(
        CreateBrandCommand command,
        CancellationToken cancellationToken)
    {
        var brandId = await sender.Send(command, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(BrandsController.GetBrandById),
            controllerName: "Brands",
            routeValues: new { id = brandId },
            value: brandId);
    }

    /// <summary>
    /// Updates the names of an existing brand. Omitted names keep their current values.
    /// </summary>
    /// <response code="204">The brand was updated successfully.</response>
    /// <response code="400">The supplied brand data is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a brand management role.</response>
    /// <response code="404">No brand exists with the supplied identifier.</response>
    /// <response code="409">A brand with the same English or Arabic name already exists.</response>
    /// <param name="id">The brand identifier.</param>
    /// <param name="request">The brand update details.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the brand is updated.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateBrand(
        Guid id,
        [FromBody] UpdateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBrandCommand(id, request.NameEn, request.NameAr);
        await sender.Send(command, cancellationToken);

        return NoContent();
    }
}
