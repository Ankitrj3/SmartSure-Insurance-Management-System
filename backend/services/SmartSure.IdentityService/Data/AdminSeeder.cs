using IdentityService.Data;
using IdentityService.Helpers;
using IdentityService.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data
{
    /// <summary>
    /// Represent or implements AdminSeeder.
    /// </summary>
    public static class AdminSeeder
    {
        /// <summary>
        /// Performs the SeedAdminAsync operation.
        /// </summary>
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();

            // Load secure credentials from configuration (appsettings.json, User Secrets, or Environment Variables)
            var adminEmail = configuration["AdminSettings:Email"];
            var adminPassword = configuration["AdminSettings:Password"];
            var adminName = configuration["AdminSettings:FullName"] ?? "System Admin";

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                // If credentials are not set securely, we don't seed the admin.
                return;
            }

            // 1. Ensure "Admin" role exists
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
            if (adminRole == null)
            {
                adminRole = new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "Admin"
                };
                context.Roles.Add(adminRole);
                await context.SaveChangesAsync();
            }

            // 2. Ensure "User" role exists while we are at it
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            if (userRole == null)
            {
                userRole = new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "User"
                };
                context.Roles.Add(userRole);
                await context.SaveChangesAsync();
            }

            // 3. Check if the Admin user exists
            var existingAdmin = await context.Users.Include(u => u.Password).Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new User
                {
                    UserId = Guid.NewGuid(),
                    FullName = adminName,
                    Email = adminEmail,
                    PhoneNumber = "0000000000",
                    Address = "HQ",
                    Password = new Password
                    {
                        PassId = Guid.NewGuid(),
                        PasswordHash = PasswordHasher.PasswordHash(adminPassword)
                    }
                };
                adminUser.Password.UserId = adminUser.UserId;

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();

                // 4. Assign Admin role to the user
                var userRoleMapping = new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.UserId,
                    RoleId = adminRole.RoleId
                };
                context.UserRoles.Add(userRoleMapping);
                await context.SaveChangesAsync();
            }
            else
            {
                // FORCE UPDATE PASSWORD
                if (existingAdmin.Password != null)
                {
                    existingAdmin.Password.PasswordHash = PasswordHasher.PasswordHash(adminPassword);
                }

                // Ensure role mapping exists
                if (existingAdmin.UserRoles == null || !existingAdmin.UserRoles.Any(ur => ur.RoleId == adminRole.RoleId))
                {
                    context.UserRoles.Add(new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = existingAdmin.UserId,
                        RoleId = adminRole.RoleId
                    });
                }
                
                await context.SaveChangesAsync();
            }
        }
    }
}
