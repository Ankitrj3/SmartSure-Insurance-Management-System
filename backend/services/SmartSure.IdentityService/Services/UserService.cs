using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Repositories;
using SmartSure.Shared.Contracts.Exceptions;

namespace IdentityService.Services
{
    /// <summary>
    /// Background service managing broad user administration tasks like mapping profiles, 
    /// cascading deletes, and role grants. Restricted to administrative workflows.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Assigns a specific role to a user. Validates existence of both user and role before assignment.
        /// </summary>
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

        /// <summary>
        /// Purges a user and their associated cascading data explicitly.
        /// Validates user existence before processing.
        /// </summary>
        public async Task DeleteUser(Guid userId)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("User", userId);

            _repo.Delete(user);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a comprehensive list of all registered users in the system.
        /// Includes standard profile data and flattens associated roles into a simplified string list.
        /// </summary>
        /// <returns>A collection of user data transfer objects.</returns>
        public async Task<List<UserDTO>> GetUsers()
        {
            var users = await _repo.GetAllAsync();
            return users.Select(u => new UserDTO
            {
                UserId      = u.UserId,
                Email       = u.Email,
                FullName    = u.FullName,
                PhoneNumber = u.PhoneNumber,
                Address     = u.Address,
                Roles       = u.UserRoles?.Where(ur => ur.Role != null)
                                         .Select(ur => ur.Role.RoleName)
                                         .ToList() ?? new List<string>()
            }).ToList();
        }
    }
}
