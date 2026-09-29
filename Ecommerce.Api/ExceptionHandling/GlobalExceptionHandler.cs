using Application.Exceptions;
using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(
            httpContext,
            exception);

        LogException(logger, exception, problemDetails.Status ?? 500);

        httpContext.Response.StatusCode =
            problemDetails.Status ?? 500;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext context,
        Exception exception)
    {
        return exception switch {
            ValidationException validationException
                => CreateValidationProblemDetails(context, validationException),

            NotFoundException ex
                => CreateProblemDetails(
                    context,
                    StatusCodes.Status404NotFound,
                    ex.Message),

            ConflictException ex
                => CreateProblemDetails(
                    context,
                    StatusCodes.Status409Conflict,
                    ex.Message),

            UnauthorizedException ex
                => CreateProblemDetails(
                    context,
                    StatusCodes.Status401Unauthorized,
                    ex.Message),

            ForbiddenException ex
                => CreateProblemDetails(
                    context,
                    StatusCodes.Status403Forbidden,
                    ex.Message),

            DbUpdateConcurrencyException
                => CreateProblemDetails(
                    context,
                    StatusCodes.Status409Conflict,
                    "The operation conflicts with a concurrent change. Please retry."),

            DomainException ex
                => CreateProblemDetails(
                    context,
                    StatusCodes.Status400BadRequest,
                    ex.Message),

            _ => CreateProblemDetails(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.")
        };
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext context,
        int statusCode,
        string detail)
    {
        return new ProblemDetails {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        ValidationException exception)
    {
        var modelState = new ModelStateDictionary();

        foreach (var error in exception.Errors) {
            foreach (var message in error.Value) {
                modelState.AddModelError(error.Key, message);
            }
        }

        return new ValidationProblemDetails(modelState) {
            Type = "https://httpstatuses.com/400",
            Title = "Validation Failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message,
            Instance = context.Request.Path
        };
    }

    private static void LogException(
        ILogger logger,
        Exception exception,
        int statusCode)
    {
        if (statusCode >= 500) {
            logger.LogError(
                exception,
                "Unhandled exception");
        }
        else {
            logger.LogWarning(
                exception,
                "Handled exception");
        }
    }
}
