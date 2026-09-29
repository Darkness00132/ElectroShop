using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Reviews.Commands.CreateReview;
using Application.Features.Reviews.Commands.DeleteReview;
using Application.Features.Reviews.Commands.UpdateMyReview;
using Application.Features.Reviews.Queries.GetProductReviews;
using Domain.Constants;
using Ecommerce.Api.Contracts.Reviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

/// <summary>
/// Provides endpoints for product reviews: public browsing, customer reviews of
/// purchased products, and support agent moderation.
/// </summary>
[ApiController]
[Route("api/reviews")]
[Produces("application/json")]
public sealed class ReviewsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves the reviews of a product, newest first.
    /// </summary>
    /// <response code="200">The reviews were retrieved successfully.</response>
    /// <param name="productId">The product identifier.</param>
    /// <param name="pagination">The pagination options.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A paginated collection of reviews.</returns>
    [AllowAnonymous]
    [HttpGet("products/{productId}")]
    [ProducesResponseType(typeof(PagedResult<Application.Features.Reviews.Dtos.ReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Application.Features.Reviews.Dtos.ReviewDto>>> GetProductReviews(
        Guid productId,
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var reviews = await sender.Send(new GetProductReviewsQuery(productId, pagination), cancellationToken);

        return Ok(reviews);
    }

    /// <summary>
    /// Reviews a product. Only customers who purchased the product can review it,
    /// and each customer can review a product once.
    /// </summary>
    /// <response code="201">The review was created successfully.</response>
    /// <response code="400">The review data is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user has not purchased the product.</response>
    /// <response code="404">The product does not exist.</response>
    /// <response code="409">The user already reviewed this product.</response>
    /// <param name="productId">The product identifier.</param>
    /// <param name="request">The rating and optional comment.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The identifier of the created review.</returns>
    [Authorize]
    [HttpPost("products/{productId}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateReview(
        Guid productId,
        CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var reviewId = await sender.Send(new CreateReviewCommand(productId, request.Rating, request.Comment), cancellationToken);

        return Created($"/api/reviews/{reviewId}", reviewId);
    }

    /// <summary>
    /// Updates the authenticated customer's own review.
    /// </summary>
    /// <response code="204">The review was updated successfully.</response>
    /// <response code="400">The review data is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="404">No review of this customer exists with the supplied identifier.</response>
    /// <param name="id">The review identifier.</param>
    /// <param name="request">The new rating and optional comment.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the review is updated.</returns>
    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyReview(
        Guid id,
        UpdateReviewRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateMyReviewCommand(id, request.Rating, request.Comment), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Removes a review. Intended for support agent moderation.
    /// </summary>
    /// <response code="204">The review was removed successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not assigned to a support role.</response>
    /// <response code="404">No review exists with the supplied identifier.</response>
    /// <param name="id">The review identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the review is removed.</returns>
    [Authorize(Roles = AppRoles.SupportUsers)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteReviewCommand(id), cancellationToken);

        return NoContent();
    }
}
