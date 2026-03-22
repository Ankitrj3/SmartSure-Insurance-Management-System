using Microsoft.EntityFrameworkCore;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Data
{
    public class PolicyDbContext : DbContext
    {
        public PolicyDbContext(DbContextOptions<PolicyDbContext> options) : base(options)
        {
        }

        public DbSet<InsuranceType> InsuranceTypes { get; set; }
        public DbSet<InsuranceSubtype> InsuranceSubtypes { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<PolicyDetail> PolicyDetails { get; set; }
        public DbSet<HomeDetail> HomeDetails { get; set; }
        public DbSet<VehicleDetail> VehicleDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Discount> Discounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<InsuranceType>()
                .HasMany(t => t.Subtypes)
                .WithOne(s => s.Type)
                .HasForeignKey(s => s.TypeId);

            modelBuilder.Entity<InsuranceSubtype>()
                .HasOne(s => s.Type)
                .WithMany(t => t.Subtypes)
                .HasForeignKey(s => s.TypeId);

            modelBuilder.Entity<Policy>()
                .HasOne(p => p.Subtype)
                .WithMany()
                .HasForeignKey(p => p.SubtypeId);

            modelBuilder.Entity<Policy>()
                .HasOne(p => p.PolicyDetail)
                .WithOne(pd => pd.Policy)
                .HasForeignKey<PolicyDetail>(pd => pd.PolicyId);

            modelBuilder.Entity<Policy>()
                .HasOne(p => p.HomeDetail)
                .WithOne(hd => hd.Policy)
                .HasForeignKey<HomeDetail>(hd => hd.PolicyId);

            modelBuilder.Entity<Policy>()
                .HasOne(p => p.VehicleDetail)
                .WithOne(vd => vd.Policy)
                .HasForeignKey<VehicleDetail>(vd => vd.PolicyId);

            modelBuilder.Entity<Policy>()
                .HasMany(p => p.Payments)
                .WithOne(py => py.Policy)
                .HasForeignKey(py => py.PolicyId);

            // Precision for decimal properties
            modelBuilder.Entity<InsuranceSubtype>()
                .Property(s => s.BasePremium)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Policy>()
                .Property(p => p.PremiumAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Policy>()
                .Property(p => p.InsuredDeclaredValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<HomeDetail>()
                .Property(h => h.EstimatedValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VehicleDetail>()
                .Property(v => v.EstimatedValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);
        }
    }
}
