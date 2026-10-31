using Banco.Application.Common.Exceptions;
using Banco.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace Banco.Api.ErrorHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int statusCode;
        string title;
        string? detail;

        switch (exception)
        {
            case DomainException:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Invalid request";
                detail = exception.Message;
                _logger.LogWarning("Domain validation failed: {Message}", exception.Message);
                break;

            case NotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                title = "Resource not found";
                detail = exception.Message;
                _logger.LogInformation("Resource not found: {Message}", exception.Message);
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                title = "An unexpected error occurred";
                detail = null;
                _logger.LogError(exception, "Unhandled exception");
                break;
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            (JsonSerializerOptions?)null,
            "application/problem+json",
            cancellationToken);

        return true;
    }
}