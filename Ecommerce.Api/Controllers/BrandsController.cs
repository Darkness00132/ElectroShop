using Application.Features.Brands.Dtos;
using Application.Features.Brands.Queries.GetBrandById;
using Application.Features.Brands.Queries.GetBrands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

/// <summary>
/// Provides public endpoints for browsing brands.
/// </summary>
[ApiController]
[Route("api/brands")]
[Produces("application/json")]
public sealed class BrandsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all brands sorted by English name.
    /// </summary>
    /// <response code="200">The brands were retrieved successfully.</response>
    /// <response code="400">The request is invalid.</response>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The collection of available brands.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<BrandDto>>> GetBrands(
        CancellationToken cancellationToken)
    {
        var brands = await sender.Send(new GetBrandsQuery(), cancellationToken);

        return Ok(brands);
    }

    /// <summary>
    /// Retrieves a single brand by its identifier.
    /// </summary>
    /// <response code="200">The brand was retrieved successfully.</response>
    /// <response code="404">No brand exists with the supplied identifier.</response>
    /// <param name="id">The brand identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The requested brand.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BrandDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrandDto>> GetBrandById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var brand = await sender.Send(new GetBrandByIdQuery(id), cancellationToken);

        return Ok(brand);
    }
}
