using Api.Contracts.Common;
using Application.Common.Filters;
using Application.Features.Products.Commands.AssignProductDiscount;
using Application.Features.Products.Commands.ActivateProduct;
using Application.Features.Products.Commands.ChangeProductPrice;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeactivateProduct;
using Application.Features.Products.Commands.RemoveProductDiscount;
using Application.Features.Products.Commands.UpdateProduct;
using AutoMapper;
using Domain.Constants;
using Ecommerce.Api.Contracts.Products;
using Ecommerce.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers.Admin;

/// <summary>
/// Provides catalog administrator endpoints for managing products.
/// </summary>
[ApiController]
[Authorize(Roles = AppRoles.CatalogAdministrators)]
[Route("api/admin/products")]
[Produces("application/json")]
public sealed class ProductManagementController(IMapper mapper, ISender sender) : ControllerBase
{
    /// <summary>
    /// Creates a new product with its gallery images and returns its identifier.
    /// </summary>
    /// <remarks>
    /// Accepts <c>multipart/form-data</c> because the gallery images are uploaded with the request.
    /// Between 1 and 5 images are required; each must be a JPG, PNG, or WebP file of at most 10 MB
    /// and is automatically resized to an 800x800 WebP image before being stored.
    /// The images are uploaded to the product gallery and an empty inventory record is created.
    /// </remarks>
    /// <response code="201">The product was created successfully.</response>
    /// <response code="400">The supplied product data or images are invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a product management role.</response>
    /// <response code="404">The category, brand, or discount does not exist.</response>
    /// <param name="request">The product creation details and images.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The identifier of the created product.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CreateProduct(
        [FromForm] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<CreateProductCommand>(request);
        var productId = await sender.Send(command, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(ProductsController.GetProductById),
            controllerName: "Products",
            routeValues: new { id = productId },
            value: productId);
    }

    /// <summary>
    /// Partially updates an existing product. Omitted fields keep their current values.
    /// </summary>
    /// <remarks>
    /// Accepts <c>multipart/form-data</c> because new gallery images may be uploaded with the request.
    /// Removed images are deleted from storage, and the supplied attributes replace the current set.
    /// </remarks>
    /// <response code="204">The product was updated successfully.</response>
    /// <response code="400">The supplied product data or images are invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a product management role.</response>
    /// <response code="404">The product, category, or brand does not exist.</response>
    /// <param name="id">The product identifier.</param>
    /// <param name="request">The product update details.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the product is updated.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromForm] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<UpdateProductCommand>(request) with { Id = id };
        await sender.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Makes a product visible in the storefront.
    /// </summary>
    /// <response code="204">The product was activated successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a product management role.</response>
    /// <response code="404">No product exists with the supplied identifier.</response>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the product is activated.</returns>
    [HttpPut("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ActivateProductCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Hides a product from the storefront.
    /// </summary>
    /// <response code="204">The product was deactivated successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a product management role.</response>
    /// <response code="404">No product exists with the supplied identifier.</response>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the product is deactivated.</returns>
    [HttpPut("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeactivateProductCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Changes the price of a product.
    /// </summary>
    /// <response code="204">The price was changed successfully.</response>
    /// <response code="400">The supplied price is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a product management role.</response>
    /// <response code="404">No product exists with the supplied identifier.</response>
    /// <param name="id">The product identifier.</param>
    /// <param name="request">The new price.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the price is changed.</returns>
    [HttpPut("{id}/price")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeProductPrice(
        Guid id,
        UpdateProductPriceRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ChangeProductPriceCommand(id, request.Price), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Assigns a discount to a product, replacing any previously assigned discount.
    /// </summary>
    /// <response code="204">The discount was assigned successfully.</response>
    /// <response code="400">The supplied discount identifier is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a product management role.</response>
    /// <response code="404">The product or discount does not exist.</response>
    /// <param name="id">The product identifier.</param>
    /// <param name="request">The discount to assign.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the discount is assigned.</returns>
    [HttpPut("{id}/discount")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignProductDiscount(
        Guid id,
        AssignProductDiscountRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new AssignProductDiscountCommand(id, request.DiscountId), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Removes the discount assigned to a product. Products without a discount are left unchanged.
    /// </summary>
    /// <response code="204">The discount was removed successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a product management role.</response>
    /// <response code="404">No product exists with the supplied identifier.</response>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the discount is removed.</returns>
    [HttpDelete("{id}/discount")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveProductDiscount(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new RemoveProductDiscountCommand(id), cancellationToken);

        return NoContent();
    }
}
