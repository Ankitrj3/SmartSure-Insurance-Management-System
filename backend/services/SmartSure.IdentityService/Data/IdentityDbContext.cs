using IdentityService.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data
{
    /// <summary>
    /// Represent or implements IdentityDbContext.
    /// </summary>
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Password> Passwords { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ExternalLogin> ExternalLogins { get; set; }
        public DbSet<OtpRecord> OtpRecords { get; set; }

        /// <summary>
        /// Performs the OnModelCreating operation.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Password>()
                .HasOne(p => p.User)
                .WithOne(u => u.Password)
                .HasForeignKey<Password>(p => p.UserId);

            modelBuilder.Entity<ExternalLogin>()
                .HasOne(el => el.User)
                .WithMany(u => u.ExternalLogins)
                .HasForeignKey(el => el.UserId);

            modelBuilder.Entity<ExternalLogin>()
                .HasIndex(el => new { el.Provider, el.ProviderKey })
                .IsUnique();

            modelBuilder.Entity<OtpRecord>()
                .HasOne(o => o.User)
                .WithMany(u => u.OtpRecords)
                .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        }
    }
}
