using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace SmartSure.Shared.Contracts.Middleware;

/// <summary>
/// Represent or implements GlobalExceptionHandlerMiddleware.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Performs the InvokeAsync operation.
    /// </summary>
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception,
            "Exception caught by global handler | Path: {Path} | Method: {Method} | Message: {Message} | StackTrace: {StackTrace}",
            context.Request.Path, context.Request.Method, exception.Message, exception.StackTrace);

        HttpStatusCode statusCode;
        string message;

        // Handle SmartSure custom exceptions first (they carry their own HTTP status code)
        if (exception is Exceptions.SmartSureException smartSureEx)
        {
            statusCode = (HttpStatusCode)smartSureEx.StatusCode;
            message    = smartSureEx.Message;
        }
        else
        {
            // Fall back to standard .NET exception types
            (statusCode, message) = exception switch
            {
                KeyNotFoundException        => (HttpStatusCode.NotFound,            exception.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,        exception.Message),
                ArgumentException           => (HttpStatusCode.BadRequest,          exception.Message),
                InvalidOperationException   => (HttpStatusCode.Conflict,            exception.Message),
                _                           => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };
        }

        // In development, expose full detail for 500 errors
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        if (isDevelopment && statusCode == HttpStatusCode.InternalServerError)
        {
            message = $"{exception.GetType().Name}: {exception.Message}";
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var response = new
        {
            success    = false,
            message,
            statusCode = (int)statusCode,
            traceId    = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
