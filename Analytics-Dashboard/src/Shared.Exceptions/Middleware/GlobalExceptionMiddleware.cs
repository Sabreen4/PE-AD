using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Common.Dtos;
using Shared.Exceptions;

namespace Shared.Middleware;

/// <summary>
/// Middleware that catches unhandled exceptions, maps known domain exceptions
/// to appropriate HTTP status codes, and returns a standardized JSON error response.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next delegate in the request pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware, wrapping the downstream pipeline in a try-catch
    /// to handle any unhandled exceptions.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles the exception by setting the appropriate HTTP status code
    /// and writing a standardized <see cref="ErrorResponseDto"/> JSON response.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case BaseException bex:
            {
                _logger.LogWarning(bex, "Domain exception: {Message}", bex.Message);

                context.Response.StatusCode = bex.StatusCode;
                ErrorResponseDto errorResponse = new ErrorResponseDto(bex.Message);
                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                break;
            }

            default:
            {
                _logger.LogError(exception, "Unhandled exception");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ErrorResponseDto errorResponse = new ErrorResponseDto("An unexpected error occurred.");
                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                break;
            }
        }
    }
}