using Application.Features.Categories.Commands.CreateCategory;
using Application.Features.Categories.Commands.UpdateCategory;
using AutoMapper;
using Domain.Constants;
using Ecommerce.Api.Contracts.Categories;
using Ecommerce.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

/// <summary>
/// Provides catalog administrator endpoints for managing categories.
/// </summary>
[ApiController]
[Authorize(Roles = AppRoles.CatalogAdministrators)]
[Route("api/admin/categories")]
[Produces("application/json")]
public sealed class CategoryManagementController(
    IMapper mapper,
    ISender sender) : ControllerBase
{
    /// <summary>
    /// Creates a new category with an image and returns its identifier.
    /// </summary>
    /// <remarks>
    /// Accepts <c>multipart/form-data</c> because the category image is uploaded with the request.
    /// The image is required: it must be a JPG, PNG, or WebP file of at most 5 MB and is automatically
    /// resized to a 500x500 WebP image before being stored.
    /// The English and Arabic names must both be unique; a conflict with any existing category is rejected.
    /// </remarks>
    /// <response code="201">The category was created successfully.</response>
    /// <response code="400">The supplied category data or image is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a category management role.</response>
    /// <response code="409">A category with the same English or Arabic name already exists.</response>
    /// <param name="request">The category creation details and image.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The identifier of the created category.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateCategory(
        [FromForm] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<CreateCategoryCommand>(request);
        var categoryId = await sender.Send(command, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(CategoriesController.GetCategoryById),
            controllerName: "Categories",
            routeValues: new { id = categoryId },
            value: categoryId);
    }

    /// <summary>
    /// Replaces the details of an existing category. The image is only replaced when a new one is supplied.
    /// </summary>
    /// <remarks>
    /// Accepts <c>multipart/form-data</c> because the category image may be uploaded with the request.
    /// This endpoint replaces the category details: all name and description fields must be supplied,
    /// and the supplied names must not belong to a different category.
    /// When <c>NewImage</c> is omitted, the current image is kept; when supplied, it must be a JPG, PNG,
    /// or WebP file of at most 5 MB, and the replaced image is removed from storage.
    /// </remarks>
    /// <response code="204">The category was updated successfully.</response>
    /// <response code="400">The supplied category data or image is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a category management role.</response>
    /// <response code="404">No category exists with the supplied identifier.</response>
    /// <response code="409">A category with the same English or Arabic name already exists.</response>
    /// <param name="id">The category identifier.</param>
    /// <param name="request">The full category update details.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the category is updated.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromForm] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<UpdateCategoryCommand>(request) with { Id = id };
        await sender.Send(command, cancellationToken);

        return NoContent();
    }
}
