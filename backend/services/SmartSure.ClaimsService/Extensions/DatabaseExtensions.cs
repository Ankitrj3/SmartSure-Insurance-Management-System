using Microsoft.EntityFrameworkCore;
using SmartSure.ClaimsService.Data;

namespace SmartSure.ClaimsService.Extensions
{
    public static class DatabaseExtensions
    {
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
            await db.Database.MigrateAsync();
        }
    }
}
