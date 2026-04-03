using IdentityService.Models;

namespace IdentityService.Repositories
{
    /// <summary>
    /// Represent or implements IUserRepository.
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<List<User>> GetAllAsync();
        Task AddAsync(User user);
        Task SaveChangesAsync();
        void Delete(User user);
        Task<Role?> GetRoleByIdAsync(Guid roleId);
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task AddUserRoleAsync(UserRole userRole);
    }
}
