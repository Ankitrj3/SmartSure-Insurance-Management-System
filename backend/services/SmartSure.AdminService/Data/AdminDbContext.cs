using Microsoft.EntityFrameworkCore;
using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Data
{
    /// <summary>
    /// Represent or implements AdminDbContext.
    /// </summary>
    public class AdminDbContext : DbContext
    {
        public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }

        public DbSet<Report> Reports { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// Performs the OnModelCreating operation.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.Timestamp);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.EntityType);

            modelBuilder.Entity<Report>()
                .HasIndex(r => r.CreatedAt);
        }
    }
}
