using Application.Features.Categories.Dtos;
using Application.Features.Categories.Queries.GetCategories;
using Application.Features.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

/// <summary>
/// Provides public endpoints for browsing categories.
/// </summary>
[ApiController]
[Route("api/categories")]
[Produces("application/json")]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all categories sorted by English name.
    /// </summary>
    /// <remarks>
    /// No authentication is required. Each category carries both the English (En) and Arabic (Ar)
    /// name and description together with its image key. The result is served from cache and is
    /// refreshed whenever a category is created or updated.
    /// </remarks>
    /// <response code="200">The categories were retrieved successfully.</response>
    /// <response code="400">The request is invalid.</response>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The collection of available categories.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var categories = await sender.Send(new GetCategoriesQuery(), cancellationToken);

        return Ok(categories);
    }

    /// <summary>
    /// Retrieves a single category by its identifier.
    /// </summary>
    /// <remarks>
    /// No authentication is required. The result is served from cache and is refreshed
    /// whenever that category is updated.
    /// </remarks>
    /// <response code="200">The category was retrieved successfully.</response>
    /// <response code="404">No category exists with the supplied identifier.</response>
    /// <param name="id">The category identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The requested category.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var category = await sender.Send(new GetCategoryByIdQuery(id), cancellationToken);

        return Ok(category);
    }
}
