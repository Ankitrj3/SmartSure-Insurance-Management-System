using Microsoft.EntityFrameworkCore;
using SmartSure.ClaimsService.Data;

namespace SmartSure.ClaimsService.Extensions
{
    /// <summary>
    /// Represent or implements DatabaseExtensions.
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Performs the ApplyMigrationsAsync operation.
        /// </summary>
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
            await db.Database.MigrateAsync();
        }
    }
}
