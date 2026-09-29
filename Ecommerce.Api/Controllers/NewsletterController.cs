using Api.Contracts.Common;
using Application.Common.Pagination;
using Application.Features.Newsletter.Commands.SubscribeToNewsletter;
using Application.Features.Newsletter.Commands.UnsubscribeFromNewsletter;
using Application.Features.Newsletter.Dtos;
using Application.Features.Newsletter.Queries.GetNewsletterSubscribers;
using Domain.Constants;
using Ecommerce.Api.Contracts.Newsletter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

/// <summary>
/// Provides public endpoints for newsletter subscription and administrator
/// endpoints for viewing the subscribers.
/// </summary>
[ApiController]
[Route("api/newsletter")]
[Produces("application/json")]
public sealed class NewsletterController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Subscribes an email address to the newsletter.
    /// </summary>
    /// <remarks>
    /// An email that previously unsubscribed is subscribed again; subscribing an already
    /// subscribed email is rejected.
    /// </remarks>
    /// <response code="204">The email was subscribed successfully.</response>
    /// <response code="400">The supplied email is invalid.</response>
    /// <response code="409">The email is already subscribed.</response>
    /// <param name="request">The email address to subscribe.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the email is subscribed.</returns>
    [AllowAnonymous]
    [HttpPost("subscribe")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Subscribe(
        SubscribeRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new SubscribeToNewsletterCommand(request.Email), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Unsubscribes an email address from the newsletter. Unknown emails are ignored.
    /// </summary>
    /// <response code="204">The email was unsubscribed successfully, or was not subscribed.</response>
    /// <param name="request">The email address to unsubscribe.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>No content when the email is unsubscribed.</returns>
    [AllowAnonymous]
    [HttpPost("unsubscribe")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Unsubscribe(
        UnsubscribeRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UnsubscribeFromNewsletterCommand(request.Email), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Retrieves all newsletter subscribers, newest first.
    /// </summary>
    /// <response code="200">The subscribers were retrieved successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user is not an administrator.</response>
    /// <param name="pagination">The pagination options.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A paginated collection of subscribers.</returns>
    [Authorize(Roles = AppRoles.Administrators)]
    [HttpGet("subscribers")]
    [ProducesResponseType(typeof(PagedResult<NewsletterSubscriberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<NewsletterSubscriberDto>>> GetSubscribers(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var subscribers = await sender.Send(new GetNewsletterSubscribersQuery(pagination), cancellationToken);

        return Ok(subscribers);
    }
}
