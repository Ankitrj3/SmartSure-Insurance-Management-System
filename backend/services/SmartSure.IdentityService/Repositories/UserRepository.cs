using IdentityService.Data;
using IdentityService.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Repositories
{
    /// <summary>
    /// Represent or implements UserRepository.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IdentityDbContext _context;
        public UserRepository(IdentityDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Performs the AddAsync operation.
        /// </summary>
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        /// <summary>
        /// Performs the GetAllAsync operation.
        /// </summary>
        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Performs the GetByEmailAsync operation.
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(x => x.Password)
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        /// <summary>
        /// Performs the GetByIdAsync operation.
        /// </summary>
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(x => x.Password)
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == id);
        }

        /// <summary>
        /// Performs the SaveChangesAsync operation.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Performs the Delete operation.
        /// </summary>
        public void Delete(User user)
        {
            _context.Users.Remove(user);
        }

        /// <summary>
        /// Performs the GetRoleByIdAsync operation.
        /// </summary>
        public async Task<Role?> GetRoleByIdAsync(Guid roleId)
        {
            return await _context.Roles.FindAsync(roleId);
        }

        /// <summary>
        /// Performs the GetRoleByNameAsync operation.
        /// </summary>
        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
        }

        /// <summary>
        /// Performs the AddUserRoleAsync operation.
        /// </summary>
        public async Task AddUserRoleAsync(UserRole userRole)
        {
            await _context.UserRoles.AddAsync(userRole);
        }
    }
}
