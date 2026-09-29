using Api.Contracts.Common;
using Application.Common.Filters;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

/// <summary>
/// Provides storefront endpoints for browsing active products.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/products")]
[Produces("application/json")]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    /// <param name="sender">
    /// The mediator sender used to dispatch application requests.
    /// </param>
    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Gets a paginated collection of active products.
    /// </summary>
    /// <param name="pagination">
    /// The pagination options.
    /// </param>
    /// <param name="filter">
    /// The product filtering and sorting options.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the request.
    /// </param>
    /// <returns>A paginated collection of active products.</returns>
    /// <response code="200">
    /// The products were retrieved successfully.
    /// </response>
    /// <response code="400">
    /// The filtering, sorting, or pagination values are invalid.
    /// </response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] PaginationRequest pagination,
        [FromQuery] ProductFilter? filter,
        CancellationToken cancellationToken)
    {
        var query = new GetProductsQuery(pagination, filter);

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets the storefront details of an active product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">
    /// A token used to cancel the request.
    /// </param>
    /// <returns>The product storefront details.</returns>
    /// <response code="200">
    /// The product was retrieved successfully.
    /// </response>
    /// <response code="404">
    /// The product was not found or is not active.
    /// </response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }
}
