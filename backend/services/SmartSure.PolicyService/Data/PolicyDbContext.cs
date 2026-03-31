using Microsoft.EntityFrameworkCore;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Data
{
    /// <summary>
    /// Represent or implements PolicyDbContext.
    /// </summary>
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

        /// <summary>
        /// Performs the OnModelCreating operation.
        /// </summary>
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

            // Seed Data
            SeedInsuranceData(modelBuilder);
        }

        private void SeedInsuranceData(ModelBuilder modelBuilder)
        {
            // Insurance Types
            var vehicleTypeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var homeTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            modelBuilder.Entity<InsuranceType>().HasData(
                new InsuranceType
                {
                    TypeId = vehicleTypeId,
                    Name = "Vehicle",
                    Description = "Comprehensive vehicle insurance coverage"
                },
                new InsuranceType
                {
                    TypeId = homeTypeId,
                    Name = "Home",
                    Description = "Complete home and property insurance"
                }
            );

            // Vehicle Insurance Subtypes (Car Companies)
            modelBuilder.Entity<InsuranceSubtype>().HasData(
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0001-0000-0000-000000000001"),
                    TypeId = vehicleTypeId,
                    Name = "Mahindra",
                    Description = "Insurance for Mahindra vehicles",
                    BasePremium = 15000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0002-0000-0000-000000000002"),
                    TypeId = vehicleTypeId,
                    Name = "Maruti Suzuki",
                    Description = "Insurance for Maruti Suzuki vehicles",
                    BasePremium = 12000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0003-0000-0000-000000000003"),
                    TypeId = vehicleTypeId,
                    Name = "Hyundai",
                    Description = "Insurance for Hyundai vehicles",
                    BasePremium = 13000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0004-0000-0000-000000000004"),
                    TypeId = vehicleTypeId,
                    Name = "Honda",
                    Description = "Insurance for Honda vehicles",
                    BasePremium = 14000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0005-0000-0000-000000000005"),
                    TypeId = vehicleTypeId,
                    Name = "Tata Motors",
                    Description = "Insurance for Tata vehicles",
                    BasePremium = 13500
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0006-0000-0000-000000000006"),
                    TypeId = vehicleTypeId,
                    Name = "Toyota",
                    Description = "Insurance for Toyota vehicles",
                    BasePremium = 16000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0007-0000-0000-000000000007"),
                    TypeId = vehicleTypeId,
                    Name = "Kia",
                    Description = "Insurance for Kia vehicles",
                    BasePremium = 14500
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0008-0000-0000-000000000008"),
                    TypeId = vehicleTypeId,
                    Name = "Volkswagen",
                    Description = "Insurance for Volkswagen vehicles",
                    BasePremium = 17000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0009-0000-0000-000000000009"),
                    TypeId = vehicleTypeId,
                    Name = "Skoda",
                    Description = "Insurance for Skoda vehicles",
                    BasePremium = 16500
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0010-0000-0000-000000000010"),
                    TypeId = vehicleTypeId,
                    Name = "Renault",
                    Description = "Insurance for Renault vehicles",
                    BasePremium = 13000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0011-0000-0000-000000000011"),
                    TypeId = vehicleTypeId,
                    Name = "Nissan",
                    Description = "Insurance for Nissan vehicles",
                    BasePremium = 14000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0012-0000-0000-000000000012"),
                    TypeId = vehicleTypeId,
                    Name = "Ford",
                    Description = "Insurance for Ford vehicles",
                    BasePremium = 15500
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0013-0000-0000-000000000013"),
                    TypeId = vehicleTypeId,
                    Name = "MG Motor",
                    Description = "Insurance for MG Motor vehicles",
                    BasePremium = 15000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0014-0000-0000-000000000014"),
                    TypeId = vehicleTypeId,
                    Name = "Jeep",
                    Description = "Insurance for Jeep vehicles",
                    BasePremium = 18000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0015-0000-0000-000000000015"),
                    TypeId = vehicleTypeId,
                    Name = "BMW",
                    Description = "Insurance for BMW vehicles",
                    BasePremium = 25000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0016-0000-0000-000000000016"),
                    TypeId = vehicleTypeId,
                    Name = "Mercedes-Benz",
                    Description = "Insurance for Mercedes-Benz vehicles",
                    BasePremium = 28000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0017-0000-0000-000000000017"),
                    TypeId = vehicleTypeId,
                    Name = "Audi",
                    Description = "Insurance for Audi vehicles",
                    BasePremium = 26000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("11111111-0018-0000-0000-000000000018"),
                    TypeId = vehicleTypeId,
                    Name = "Volvo",
                    Description = "Insurance for Volvo vehicles",
                    BasePremium = 24000
                }
            );

            // Home Insurance Subtypes
            modelBuilder.Entity<InsuranceSubtype>().HasData(
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("22222222-0001-0000-0000-000000000001"),
                    TypeId = homeTypeId,
                    Name = "Apartment",
                    Description = "Insurance for apartments and flats",
                    BasePremium = 8000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("22222222-0002-0000-0000-000000000002"),
                    TypeId = homeTypeId,
                    Name = "Independent House",
                    Description = "Insurance for independent houses",
                    BasePremium = 12000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("22222222-0003-0000-0000-000000000003"),
                    TypeId = homeTypeId,
                    Name = "Villa",
                    Description = "Insurance for villas and luxury homes",
                    BasePremium = 18000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("22222222-0004-0000-0000-000000000004"),
                    TypeId = homeTypeId,
                    Name = "Bungalow",
                    Description = "Insurance for bungalows",
                    BasePremium = 15000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("22222222-0005-0000-0000-000000000005"),
                    TypeId = homeTypeId,
                    Name = "Penthouse",
                    Description = "Insurance for penthouses",
                    BasePremium = 20000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("22222222-0006-0000-0000-000000000006"),
                    TypeId = homeTypeId,
                    Name = "Studio Apartment",
                    Description = "Insurance for studio apartments",
                    BasePremium = 6000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("22222222-0007-0000-0000-000000000007"),
                    TypeId = homeTypeId,
                    Name = "Duplex",
                    Description = "Insurance for duplex homes",
                    BasePremium = 14000
                },
                new InsuranceSubtype
                {
                    SubtypeId = Guid.Parse("22222222-0008-0000-0000-000000000008"),
                    TypeId = homeTypeId,
                    Name = "Farmhouse",
                    Description = "Insurance for farmhouses",
                    BasePremium = 16000
                }
            );
        }
    }
}
