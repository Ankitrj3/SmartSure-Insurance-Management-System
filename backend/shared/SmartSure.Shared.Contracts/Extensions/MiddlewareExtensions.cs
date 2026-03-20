using Microsoft.AspNetCore.Builder;
using SmartSure.Shared.Contracts.Middleware;

namespace SmartSure.Shared.Contracts.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
