using System.Text.Json;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
// Alias the FluentValidation one so the switch below can tell them apart.
using FluentValidationException = FluentValidation.ValidationException;

namespace API.MiddleWare;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

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

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Build an RFC 7807 ProblemDetails (or ValidationProblemDetails) per exception type.
        ProblemDetails problem = exception switch
        {
            // FluentValidation failures from the MediatR pipeline -> 400 with per-field errors.
            FluentValidationException fve => new ValidationProblemDetails(
                fve.Errors
                   .GroupBy(e => e.PropertyName)
                   .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title  = "One or more validation errors occurred.",
                Type   = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
            },

            // Domain value-object validation (e.g. Email.Create) -> 400, single message.
            ValidationException ve     => Problem(StatusCodes.Status400BadRequest,  "Validation failed",      ve.Message),
            AuthenticationException ae => Problem(StatusCodes.Status401Unauthorized, "Authentication failed",  ae.Message),
            NotFoundException ne       => Problem(StatusCodes.Status404NotFound,     "Resource not found",     ne.Message),
            // Upstream LLM provider failed (or agent loop hit its safety cap) -> 502 Bad Gateway, not a generic 500.
            LlmUnavailableException le => Problem(StatusCodes.Status502BadGateway,   "AI provider unavailable", le.Message),
            UnknownException ue        => Problem(StatusCodes.Status500InternalServerError, "Server error",   ue.Message),
            _                          => Problem(StatusCodes.Status500InternalServerError, "Server error",   "Internal Server Error")
        };

        var statusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        if (statusCode >= 500)
            _logger.LogError(exception, "Unhandled exception at {Path}", context.Request.Path);
        else
            _logger.LogWarning(exception, "Handled domain exception at {Path}: {Message}", context.Request.Path, exception.Message);

        problem.Instance = context.Request.Path;

        // Echo the per-request correlation id (set by CorrelationIdMiddleware) into the body
        // grep the structured logs for the exact failing request.
        if (context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var correlationId)
            && correlationId is string id)
        {
            problem.Extensions["correlationId"] = id;
        }

        // application/problem+json is the media type RFC 7807 mandates.
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(problem, problem.GetType()));
    }

    private static ProblemDetails Problem(int status, string title, string detail) => new()
    {
        Status = status,
        Title  = title,
        Detail = detail
    };
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
