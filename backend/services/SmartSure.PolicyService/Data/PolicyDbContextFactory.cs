using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SmartSure.PolicyService.Data
{
    /// <summary>
    /// Represent or implements PolicyDbContextFactory.
    /// </summary>
    public class PolicyDbContextFactory : IDesignTimeDbContextFactory<PolicyDbContext>
    {
        /// <summary>
        /// Performs the CreateDbContext operation.
        /// </summary>
        public PolicyDbContext CreateDbContext(string[] args)
        {
            // Load .env file from the project directory
            var projectPath = Directory.GetCurrentDirectory();
            var envPath = Path.Combine(projectPath, ".env");
            
            if (File.Exists(envPath))
            {
                DotNetEnv.Env.Load(envPath);
            }
            
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(projectPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            
            var connectionString = configuration.GetConnectionString("PolicyConnDb");
            
            if (string.IsNullOrEmpty(connectionString) || connectionString == "PLACEHOLDER_LOADED_FROM_ENV")
            {
                connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PolicyConnDb") 
                    ?? "Data Source=localhost;Initial Catalog=smartPolicyService;Integrated Security=True;Pooling=False;Encrypt=False;TrustServerCertificate=True;";
            }

            Console.WriteLine($"[PolicyDbContextFactory] Using connection string: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");

            var optionsBuilder = new DbContextOptionsBuilder<PolicyDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new PolicyDbContext(optionsBuilder.Options);
        }
    }
}
