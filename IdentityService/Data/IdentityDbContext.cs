using IdentityService.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data
{
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Password> Passwords { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Password>()
                .HasOne(p => p.User)
                .WithOne(u => u.Password)
                .HasForeignKey<Password>(p => p.UserId);
        }
    }
}
