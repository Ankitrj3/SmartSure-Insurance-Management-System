using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Repositories;
using SmartSure.Shared.Contracts.Exceptions;

namespace IdentityService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task AssignRole(Guid userId, Guid roleId)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("User", userId);

            var role = await _repo.GetRoleByIdAsync(roleId);
            if (role == null) throw new NotFoundException("Role", roleId);

            var userRole = new UserRole { UserId = userId, RoleId = roleId };
            await _repo.AddUserRoleAsync(userRole);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteUser(Guid userId)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("User", userId);

            _repo.Delete(user);
            await _repo.SaveChangesAsync();
        }

        public async Task<List<UserDTO>> GetUsers()
        {
            var users = await _repo.GetAllAsync();
            return users.Select(u => new UserDTO
            {
                UserId   = u.UserId,
                Email    = u.Email,
                FullName = u.FullName
            }).ToList();
        }
    }
}
