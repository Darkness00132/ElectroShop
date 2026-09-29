using Api.Contracts.Common;
using Application.Common.Pagination;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

/// <summary>
/// Provides procurement staff endpoints for managing suppliers.
/// </summary>
[ApiController]
[Authorize(Roles = Domain.Constants.AppRoles.ProcurementAdministrators)]
[Route("api/admin/suppliers")]
[Produces("application/json")]
public sealed class SupplierManagementController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Creates a new supplier and returns its identifier.
    /// </summary>
    /// <response code="201">The supplier was created successfully.</response>
    /// <response code="400">The supplied supplier data is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <param name="command">The supplier creation details.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The identifier of the created supplier.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> CreateSupplier(
        Application.Features.Procurement.Suppliers.Commands.CreateSupplier.CreateSupplierCommand command,
        CancellationToken cancellationToken)
    {
        var supplierId = await sender.Send(command, cancellationToken);

        return Created($"/api/admin/suppliers/{supplierId}", supplierId);
    }

    /// <summary>
    /// Retrieves all suppliers ordered by name.
    /// </summary>
    /// <response code="200">The suppliers were retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <param name="pagination">The pagination options.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A paginated collection of suppliers.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Application.Features.Procurement.Suppliers.Dtos.SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<Application.Features.Procurement.Suppliers.Dtos.SupplierDto>>> GetSuppliers(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var suppliers = await sender.Send(new Application.Features.Procurement.Suppliers.Queries.GetSuppliers.GetSuppliersQuery(pagination), cancellationToken);

        return Ok(suppliers);
    }

    /// <summary>
    /// Retrieves a single supplier by its identifier.
    /// </summary>
    /// <response code="200">The supplier was retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">No supplier exists with the supplied identifier.</response>
    /// <param name="id">The supplier identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The supplier.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Application.Features.Procurement.Suppliers.Dtos.SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Application.Features.Procurement.Suppliers.Dtos.SupplierDto>> GetSupplierById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var supplier = await sender.Send(new Application.Features.Procurement.Suppliers.Queries.GetSupplierById.GetSupplierByIdQuery(id), cancellationToken);

        return Ok(supplier);
    }

    /// <summary>
    /// Replaces the details of an existing supplier.
    /// </summary>
    /// <response code="204">The supplier was updated successfully.</response>
    /// <response code="400">The supplied supplier data is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">No supplier exists with the supplied identifier.</response>
    /// <param name="id">The supplier identifier.</param>
    /// <param name="command">The full supplier update details.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the supplier is updated.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplier(
        Guid id,
        Application.Features.Procurement.Suppliers.Commands.UpdateSupplier.UpdateSupplierCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Reactivates a supplier.
    /// </summary>
    /// <response code="204">The supplier was activated successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">No supplier exists with the supplied identifier.</response>
    /// <param name="id">The supplier identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the supplier is activated.</returns>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateSupplier(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new Application.Features.Procurement.Suppliers.Commands.ActivateSupplier.ActivateSupplierCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deactivates a supplier without deleting its purchase order history.
    /// </summary>
    /// <response code="204">The supplier was deactivated successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a procurement role.</response>
    /// <response code="404">No supplier exists with the supplied identifier.</response>
    /// <param name="id">The supplier identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the supplier is deactivated.</returns>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateSupplier(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new Application.Features.Procurement.Suppliers.Commands.DeactivateSupplier.DeactivateSupplierCommand(id), cancellationToken);

        return NoContent();
    }
}
