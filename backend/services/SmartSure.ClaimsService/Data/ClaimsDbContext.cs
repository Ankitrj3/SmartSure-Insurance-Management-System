using Microsoft.EntityFrameworkCore;
using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Data
{
    /// <summary>
    /// Represent or implements ClaimsDbContext.
    /// </summary>
    public class ClaimsDbContext : DbContext
    {
        public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : base(options) { }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimDocument> ClaimDocuments { get; set; }
        public DbSet<ClaimStatusHistory> ClaimStatusHistory { get; set; }

        /// <summary>
        /// Performs the OnModelCreating operation.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Claim>()
                .HasMany(c => c.Documents)
                .WithOne(d => d.Claim)
                .HasForeignKey(d => d.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Claim>()
                .HasMany(c => c.StatusHistory)
                .WithOne(h => h.Claim)
                .HasForeignKey(h => h.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Claim>()
                .Property(c => c.ClaimAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Claim>()
                .Property(c => c.ApprovedAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Claim>()
                .HasIndex(c => c.UserId);

            modelBuilder.Entity<Claim>()
                .HasIndex(c => c.PolicyId);
        }
    }
}
