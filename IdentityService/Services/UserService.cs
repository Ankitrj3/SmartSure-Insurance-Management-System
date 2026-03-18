using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Repositories;

namespace IdentityService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IdentityDbContext _context;

        public UserService(IUserRepository repo, IdentityDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task AssignRole(Guid userId, Guid roleId)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) throw new Exception("Role not found");

            var userRole = new UserRole { UserId = userId, RoleId = roleId };
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(Guid userId)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            _repo.Delete(user);
            await _repo.SaveChangesAsync();
        }

        public async Task<List<UserDTO>> GetUsers()
        {
            var users = await _repo.GetAllAsync();
            return users.Select(u => new UserDTO
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = u.FullName
            }).ToList();
        }
    }
}
