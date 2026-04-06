using Microsoft.AspNetCore.Builder;
using SmartSure.Shared.Contracts.Middleware;

namespace SmartSure.Shared.Contracts.Extensions;

/// <summary>
/// Represent or implements MiddlewareExtensions.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Performs the UseGlobalExceptionHandler operation.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }

    /// <summary>
    /// Adds CorrelationId injection to the HTTP Request Pipeline.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
