using Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception occurred.");

        var problemDetails = new ProblemDetails
        {
            Status = GetStatusCode(exception),
            Title = GetTitle(exception),
            Detail = GetDetail(exception),
            // 🌟 ENHANCEMENT: Prepend the HTTP method (GET, POST, etc.) for easier client debugging
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ConcurrencyException => StatusCodes.Status409Conflict,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetTitle(Exception exception)
    {
        return exception switch
        {
            ConcurrencyException => "Concurrency conflict",
            ArgumentException => "Invalid request",
            InvalidOperationException => "Invalid operation",
            KeyNotFoundException => "Resource not found",
            _ => "An unexpected error occurred"
        };
    }

    private static string GetDetail(Exception exception)
    {
        return exception switch
        {
            _ when exception is ConcurrencyException
                or ArgumentException
                or InvalidOperationException
                or KeyNotFoundException
                => exception.Message,

            _ => "An unexpected error occurred."
        };
    }
}
