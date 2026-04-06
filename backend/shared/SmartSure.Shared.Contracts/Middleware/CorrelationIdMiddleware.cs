using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace SmartSure.Shared.Contracts.Middleware;

/// <summary>
/// Extracts or generates a Correlation ID and pushes it into the Serilog LogContext.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetCorrelationId(context);
        
        // Push the CorrelationId to Serilog's LogContext for the duration of the request
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            // Add the CorrelationId to the response headers
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
                {
                    context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);
                }
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }

    private string GetCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationId))
        {
            var value = correlationId.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return Guid.NewGuid().ToString();
    }
}
